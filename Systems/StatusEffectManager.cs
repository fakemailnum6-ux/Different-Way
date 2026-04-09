using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using DifferentWay.Systems.DataModels;
using DifferentWay.Core;

namespace DifferentWay.Systems
{
    public class ActiveEffect
    {
        public StatusEffectData Data { get; set; }
        public int RemainingDuration { get; set; }
        public int Stacks { get; set; } = 1;
    }

    public partial class StatusEffectManager : Node
    {
        private DataManager _dataManager;

        public override void _Ready()
        {
            _dataManager = GetNodeOrNull<DataManager>("/root/DataManager");
        }

        public void ApplyEffect(CharacterStats target, Dictionary<string, ActiveEffect> activeEffects, string effectId)
        {
            if (_dataManager == null || !_dataManager.StatusEffects.ContainsKey(effectId)) return;

            var newEffectData = _dataManager.StatusEffects[effectId];

            // Handle Mutual Cancellation
            if (!string.IsNullOrEmpty(newEffectData.CancelBy))
            {
                if (activeEffects.ContainsKey(newEffectData.CancelBy))
                {
                    // They cancel each other out
                    activeEffects.Remove(newEffectData.CancelBy);
                    return;
                }
            }

            // Check for existing effect to handle stack rules
            if (activeEffects.ContainsKey(effectId))
            {
                var existing = activeEffects[effectId];
                if (newEffectData.StackRule == "stackable")
                {
                    existing.Stacks++;
                    if (existing.Stacks > newEffectData.MaxStacks) existing.Stacks = newEffectData.MaxStacks;
                    ResetDuration(existing);
                }
                else if (newEffectData.StackRule == "refresh")
                {
                    ResetDuration(existing);
                }
                else if (newEffectData.StackRule == "diminishing_returns")
                {
                    // E.g., Stun immunity
                    if (activeEffects.ContainsKey($"{effectId}_immunity"))
                    {
                        return; // Blocked by immunity
                    }
                    ResetDuration(existing);
                }
            }
            else
            {
                // New application
                var active = new ActiveEffect { Data = newEffectData };
                ResetDuration(active);
                activeEffects[effectId] = active;
            }

            RecalculateModifiers(target, activeEffects);
        }

        public void TickEffects(CharacterStats target, Dictionary<string, ActiveEffect> activeEffects, EventBus eventBus, string targetName)
        {
            var keysToRemove = new List<string>();

            foreach (var kvp in activeEffects)
            {
                var effect = kvp.Value;

                // Apply per-turn damage/healing
                int dmg = effect.Data.EffectHpTurn;
                dmg += effect.Data.EffectHpTurnPerStack * effect.Stacks;

                if (dmg != 0)
                {
                    target.CurrentHP += dmg; // Note: debuffs have negative EffectHpTurn
                    string action = dmg > 0 ? "восстановил" : "потерял";
                    eventBus?.EmitLogMessage("INFO", $"{targetName} {action} {Mathf.Abs(dmg)} HP от эффекта {effect.Data.Name}.");
                }

                // Handle duration
                if (effect.Data.Duration is JsonElement je && je.ValueKind == JsonValueKind.Number)
                {
                   effect.RemainingDuration--;
                   if (effect.RemainingDuration <= 0)
                   {
                       keysToRemove.Add(kvp.Key);

                       // Handle post-immunity (e.g. stun)
                       if (effect.Data.StackRule == "diminishing_returns" && effect.Data.PostImmunityDuration > 0)
                       {
                           // Add an invisible immunity buff
                           // (Implementation detail skipped for brevity, would insert a dummy effect here)
                       }
                   }
                }
            }

            foreach (var key in keysToRemove)
            {
                activeEffects.Remove(key);
            }

            if (keysToRemove.Count > 0)
            {
                 RecalculateModifiers(target, activeEffects);
            }
        }

        public void RecalculateModifiers(CharacterStats target, Dictionary<string, ActiveEffect> activeEffects)
        {
            // Reset to base
            target.StatModifier_STR = 0;
            target.StatModifier_DEX = 0;
            target.EvasionModifier = 0;

            foreach (var effect in activeEffects.Values)
            {
                target.StatModifier_STR += effect.Data.EffectStr;
                target.StatModifier_STR += effect.Data.EffectStrPerStack * effect.Stacks;

                target.StatModifier_DEX += effect.Data.EffectDex;
                target.StatModifier_DEX += effect.Data.EffectDexPerStack * effect.Stacks;

                if (effect.Data.ZeroEvasion)
                {
                    target.EvasionModifier = -999; // effectively zeroed in StatManager
                }
            }
        }

        private void ResetDuration(ActiveEffect effect)
        {
            if (effect.Data.Duration is JsonElement je)
            {
                if (je.ValueKind == JsonValueKind.Number)
                {
                    effect.RemainingDuration = je.GetInt32();
                }
                else
                {
                    effect.RemainingDuration = 999; // "until_rest"
                }
            }
            else
            {
                // Fallback if not JsonElement (e.g. parsed directly as string/int by other deserializer)
                effect.RemainingDuration = 999;
            }
        }
    }
}
