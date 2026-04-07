using System.Collections.Generic;

namespace DifferentWay.Systems;

public class StatusEffect
{
    public string Name { get; set; } = string.Empty;
    public int Duration { get; set; }
    public int DotDamage { get; set; }
}

public partial class StatusEffectManager : Godot.RefCounted
{
    private List<StatusEffect> _activeEffects = new();

    public void ApplyEffect(StatusEffect effect)
    {
        _activeEffects.Add(effect);
    }

    public void RemoveEffectByName(string name)
    {
        for (int i = _activeEffects.Count - 1; i >= 0; i--)
        {
            if (_activeEffects[i].Name == name)
            {
                _activeEffects.RemoveAt(i);
                DifferentWay.Core.GameLogger.Log($"Статус '{name}' был излечен.");
            }
        }
    }

    public void ProcessTurn(StatManager stats, string entityName)
    {
        for (int i = _activeEffects.Count - 1; i >= 0; i--)
        {
            var effect = _activeEffects[i];
            effect.Duration--;

            if (effect.DotDamage > 0)
            {
                stats.ApplyDamage(effect.DotDamage);
                DifferentWay.Core.GameLogger.Log($"{entityName} получает {effect.DotDamage} урона от статуса '{effect.Name}'.");
            }

            if (effect.Duration <= 0)
            {
                DifferentWay.Core.GameLogger.Log($"Статус '{effect.Name}' спал с {entityName}.");
                _activeEffects.RemoveAt(i);
            }
        }
    }
}
