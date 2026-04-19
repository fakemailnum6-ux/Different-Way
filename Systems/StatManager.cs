using Godot;
using System;

namespace DifferentWay.Systems
{
    // The core stats of a character (player or NPC)
    public class CharacterStats
    {
        // Primary
        public int STR { get; set; } = 10;
        public int DEX { get; set; } = 10;
        public int END { get; set; } = 10;
        public int INT { get; set; } = 10;
        public int Luck { get; set; } = 5;

        // Social
        public int Charisma { get; set; } = 10;
        public int Karma { get; set; } = 0;

        // Computed/Pools
        public int MaxHP { get; set; }
        public int CurrentHP { get; set; }

        public int MaxStamina { get; set; }
        public int CurrentStamina { get; set; }

        public int MaxMana { get; set; }
        public int CurrentMana { get; set; }

        public int Evasion { get; set; }
        public int Cardio { get; set; }
        public int MentalRes { get; set; } = 10;

        // Active Status Effects & Buffs (Simplification for calculation)
        public int StatModifier_STR { get; set; } = 0;
        public int StatModifier_DEX { get; set; } = 0;
        public int StatModifier_END { get; set; } = 0;
        public int StatModifier_INT { get; set; } = 0;
        public int EvasionModifier { get; set; } = 0;
    }

    public partial class StatManager : Node
    {
        public override void _Ready()
        {
        }

        // Recalculates all secondary stats based on primary stats and modifiers
        public void RecalculateStats(CharacterStats stats)
        {
            // Effective stats after buffs/debuffs
            int effStr = Math.Max(1, stats.STR + stats.StatModifier_STR);
            int effDex = Math.Max(1, stats.DEX + stats.StatModifier_DEX);
            int effEnd = Math.Max(1, stats.END + stats.StatModifier_END);
            int effInt = Math.Max(1, stats.INT + stats.StatModifier_INT);

            // HP: (END * 5) + (STR * 2)
            int oldMaxHp = stats.MaxHP;
            stats.MaxHP = (effEnd * 5) + (effStr * 2);
            // Adjust current HP proportionally or clamp it
            if (stats.CurrentHP > stats.MaxHP) stats.CurrentHP = stats.MaxHP;
            if (oldMaxHp == 0) stats.CurrentHP = stats.MaxHP; // Initial setup

            // Stamina: (END * 5) + (DEX * 2)
            int oldMaxStamina = stats.MaxStamina;
            stats.MaxStamina = (effEnd * 5) + (effDex * 2);
            if (stats.CurrentStamina > stats.MaxStamina) stats.CurrentStamina = stats.MaxStamina;
            if (oldMaxStamina == 0) stats.CurrentStamina = stats.MaxStamina; // Initial setup

            // Mana: (INT * 5)
            int oldMaxMana = stats.MaxMana;
            stats.MaxMana = (effInt * 5);
            if (stats.CurrentMana > stats.MaxMana) stats.CurrentMana = stats.MaxMana;
            if (oldMaxMana == 0) stats.CurrentMana = stats.MaxMana; // Initial setup

            // Evasion: (DEX * 1.5) + (Luck * 0.2)
            stats.Evasion = (int)(effDex * 1.5f + stats.Luck * 0.2f) + stats.EvasionModifier;

            // Cardio: END + (STR * 0.5)
            stats.Cardio = (int)(effEnd + effStr * 0.5f);
        }
    }
}
