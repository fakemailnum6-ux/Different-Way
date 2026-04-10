using Godot;
using System.Collections.Generic;
using System.Linq;

public class ActiveEffect
{
    public string Id { get; set; }
    public int Stacks { get; set; }
    public int Duration { get; set; }
}

public partial class StatusEffectManager : RefCounted
{
    private List<ActiveEffect> _activeEffects = new List<ActiveEffect>();

    // Returns a copy of current effects
    public IReadOnlyList<ActiveEffect> GetActiveEffects() => _activeEffects.AsReadOnly();

    public void ApplyEffect(string effectId, int duration, bool isStackable = false, int maxStacks = 3)
    {
        // Rule 5: Mutual Cancellation (Горение vs Намокание)
        if (effectId == "burning" && HasEffect("wet"))
        {
            RemoveEffect("wet");
            ServiceLocator.Logger.LogInfo("StatusEffectManager: Burning cancelled Wet.");
            return;
        }
        if (effectId == "wet" && HasEffect("burning"))
        {
            RemoveEffect("burning");
            ServiceLocator.Logger.LogInfo("StatusEffectManager: Wet cancelled Burning.");
            return;
        }

        // Rule 4: CC Diminishing Returns (Stun Immunity)
        if (effectId == "stun" && HasEffect("stun_immunity"))
        {
            ServiceLocator.Logger.LogInfo("StatusEffectManager: Target is immune to stun this turn.");
            return;
        }

        var existingEffect = _activeEffects.FirstOrDefault(e => e.Id == effectId);

        if (existingEffect != null)
        {
            if (isStackable)
            {
                // Rule 3: Stacking
                existingEffect.Stacks = System.Math.Min(existingEffect.Stacks + 1, maxStacks);
                existingEffect.Duration = duration; // Refresh duration on stack
                ServiceLocator.Logger.LogInfo($"StatusEffectManager: Stacked {effectId} to {existingEffect.Stacks}.");
            }
            else
            {
                // Rule 2: Refresh Rule
                existingEffect.Duration = duration;
                ServiceLocator.Logger.LogInfo($"StatusEffectManager: Refreshed {effectId} duration.");
            }
        }
        else
        {
            // Rule 1: Coexistence (Just add it)
            _activeEffects.Add(new ActiveEffect { Id = effectId, Stacks = 1, Duration = duration });
            ServiceLocator.Logger.LogInfo($"StatusEffectManager: Applied {effectId}.");
        }
    }

    public void AdvanceTurn(GameState_Live liveState)
    {
        for (int i = _activeEffects.Count - 1; i >= 0; i--)
        {
            var effect = _activeEffects[i];

            // Apply DoT Math
            ProcessTick(effect, liveState);

            effect.Duration--;
            if (effect.Duration <= 0)
            {
                HandleEffectExpiration(effect);
                _activeEffects.RemoveAt(i);
            }
        }
    }

    private void ProcessTick(ActiveEffect effect, GameState_Live liveState)
    {
        switch (effect.Id)
        {
            case "poison":
                // 3 HP per turn, ignores armor
                liveState.PlayerHP -= 3;
                ServiceLocator.Logger.LogInfo("StatusEffectManager: Poison ticked for 3 damage.");
                break;
            case "bleed":
                // 2 HP per stack
                liveState.PlayerHP -= (2 * effect.Stacks);
                ServiceLocator.Logger.LogInfo($"StatusEffectManager: Bleed ticked for {2 * effect.Stacks} damage.");
                break;
            case "burning":
                // 5 HP per turn
                liveState.PlayerHP -= 5;
                ServiceLocator.Logger.LogInfo("StatusEffectManager: Burning ticked for 5 damage.");
                break;
            case "regen":
                liveState.PlayerHP += 4;
                ServiceLocator.Logger.LogInfo("StatusEffectManager: Regen healed 4 HP.");
                break;
        }
    }

    private void HandleEffectExpiration(ActiveEffect effect)
    {
        ServiceLocator.Logger.LogInfo($"StatusEffectManager: {effect.Id} expired.");

        // CC Immunity Rule
        if (effect.Id == "stun")
        {
            ApplyEffect("stun_immunity", 1);
        }
    }

    public bool HasEffect(string effectId)
    {
        return _activeEffects.Any(e => e.Id == effectId);
    }

    public void RemoveEffect(string effectId)
    {
        _activeEffects.RemoveAll(e => e.Id == effectId);
    }

    public void ClearAllEffects()
    {
        _activeEffects.Clear();
    }
}
