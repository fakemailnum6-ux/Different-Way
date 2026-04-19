using Godot;
using System;
using DifferentWay.Systems.DataModels;

namespace DifferentWay.Systems
{
    public class CombatContext
    {
        public CharacterStats Attacker { get; set; }
        public CharacterStats Defender { get; set; }
        public WeaponData Weapon { get; set; }
        public ArmorData DefenderArmor { get; set; }
    }

    public partial class DamageCalculator : Node
    {
        private Random _rng = new Random();

        public override void _Ready()
        {
        }

        public int CalculateInitiative(CharacterStats stats)
        {
            return stats.DEX + _rng.Next(1, 11); // DEX + Random(1,10)
        }

        public bool CheckHit(CombatContext ctx)
        {
            // Base Accuracy: 80%
            // Attacker Acc: (DEX * 1.5) + (Luck * 0.2)
            // Defender Ev: stats.Evasion (already calculated)

            float attackerAcc = (ctx.Attacker.DEX * 1.5f) + (ctx.Attacker.Luck * 0.2f);
            float hitChance = 80 + attackerAcc - ctx.Defender.Evasion;

            // Clamp 5% - 100%
            hitChance = Math.Clamp(hitChance, 5, 100);

            int roll = _rng.Next(1, 101);
            return roll <= hitChance;
        }

        public (int damage, bool isCrit, bool isCritFail) CalculateDamage(CombatContext ctx)
        {
            // Crit Check
            int weaponCritBonus = ctx.Weapon?.CritBonus ?? 0;
            float critChance = 5 + (ctx.Attacker.Luck * 1.0f) + weaponCritBonus;

            int roll = _rng.Next(1, 101);
            bool isCritFail = roll <= 2; // Crit fail on 1-2
            if (isCritFail)
            {
                return (0, false, true);
            }

            bool isCrit = roll <= critChance;

            // Raw Damage
            float weaponBaseDmg = ctx.Weapon?.Damage ?? 1; // Default unarmed damage = 1
            float scaleBonus = 0;

            // Simplified scaling: Melee -> STR, Ranged/Dagger -> DEX, Magic -> INT
            if (ctx.Weapon?.Type == "melee") scaleBonus = ctx.Attacker.STR * 0.5f;
            else if (ctx.Weapon?.Type == "dagger" || ctx.Weapon?.Type == "ranged") scaleBonus = ctx.Attacker.DEX * 0.5f;
            else if (ctx.Weapon?.Type == "magic") scaleBonus = ctx.Attacker.INT * 0.5f;
            else scaleBonus = ctx.Attacker.STR * 0.5f; // Unarmed

            float rawDamage = weaponBaseDmg + scaleBonus;

            // Variance +/- 10%
            float variance = (float)(0.9 + (_rng.NextDouble() * 0.2));
            rawDamage *= variance;

            if (isCrit) rawDamage *= 1.5f;

            // Armor Reduction (Flat)
            int armorValue = ctx.DefenderArmor?.Armor ?? 0;
            int armorPenetration = ctx.Weapon?.ArmorPenetration ?? 0;

            int effectiveArmor = Math.Max(0, armorValue - armorPenetration);

            int finalDamage = Math.Max(1, (int)rawDamage - effectiveArmor);

            return (finalDamage, isCrit, false);
        }

        public bool CheckFlee(CharacterStats escaper, CharacterStats pursuer)
        {
             // (DEX + Cardio) > (DEX + Cardio)
             int escaperScore = escaper.DEX + escaper.Cardio;
             int pursuerScore = pursuer.DEX + pursuer.Cardio;
             return escaperScore > pursuerScore;
        }
    }
}
