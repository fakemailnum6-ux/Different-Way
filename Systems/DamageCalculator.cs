using System;

namespace DifferentWay.Systems;

public static class DamageCalculator
{
    private static readonly Random Rnd = new Random();

    public enum HitResult
    {
        Miss,
        Hit,
        CriticalHit,
        CriticalMiss
    }

    public struct DamageResult
    {
        public int FinalDamage;
        public HitResult ResultType;
        public bool ArmorPenetrated;
    }

    public static DamageResult CalculateMeleeAttack(int attackerStr, int attackerDex, int attackerLuck, int baseWeaponDamage, int targetArmor, float targetEvasion, bool ignoresVanguard = false)
    {
        DamageResult result = new DamageResult();

        // Crit calculations (simplified logic based on Luck)
        int roll = Rnd.Next(1, 101);
        int critChance = 5 + (attackerLuck / 2);
        int critFailChance = Math.Max(1, 5 - (attackerLuck / 4));

        if (roll <= critFailChance)
        {
            result.ResultType = HitResult.CriticalMiss;
            result.FinalDamage = 0;
            return result;
        }

        // Accuracy vs Evasion (simplified)
        float accuracy = 70 + attackerDex;
        if (Rnd.Next(1, 101) > (accuracy - targetEvasion))
        {
            result.ResultType = HitResult.Miss;
            result.FinalDamage = 0;
            return result;
        }

        if (roll >= 100 - critChance)
            result.ResultType = HitResult.CriticalHit;
        else
            result.ResultType = HitResult.Hit;

        // Base damage calculation: weapon damage + STR scaling
        float rawDamage = baseWeaponDamage + (attackerStr * 0.5f);

        // +/- 10% variability
        float variance = 1.0f + ((Rnd.Next(-10, 11)) / 100f);
        rawDamage *= variance;

        if (result.ResultType == HitResult.CriticalHit)
            rawDamage *= 1.5f;

        // Apply Armor
        int finalDamage = (int)Math.Max(1, rawDamage - targetArmor);

        // Armor Penetration check (STR vs Armor)
        if (attackerStr > targetArmor * 2)
        {
            result.ArmorPenetrated = true;
            // Additional damage or effect could be applied here
        }

        result.FinalDamage = finalDamage;
        return result;
    }
}
