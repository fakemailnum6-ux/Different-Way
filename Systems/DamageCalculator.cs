using System;

public static class DamageCalculator
{
    public static int CalculateInitiative(int dex)
    {
        return dex + RNGManager.RollInt(1, 10);
    }

    public static bool CalculateHitSuccess(int attackerDex, int attackerLuck, int defenderDex, int defenderLuck)
    {
        const int BaseAccuracy = 80;
        double attackerAccuracy = (attackerDex * 1.5) + (attackerLuck * 0.2);
        double defenderEvasion = (defenderDex * 1.5) + (defenderLuck * 0.2);

        int hitChance = (int)Math.Round(BaseAccuracy + attackerAccuracy - defenderEvasion);
        hitChance = Math.Clamp(hitChance, 5, 100);

        int roll = RNGManager.RollInt(1, 100);
        return roll <= hitChance;
    }

    public static bool CheckCritFail()
    {
        return RNGManager.RollInt(1, 100) <= 2;
    }

    public static bool CalculateCritSuccess(int attackerLuck, int weaponCritBonus = 0)
    {
        int baseCrit = 5;
        int critChance = baseCrit + (attackerLuck * 1) + weaponCritBonus;

        int roll = RNGManager.RollInt(1, 100);
        return roll <= critChance;
    }

    /// <summary>
    /// weaponScalingStat is STR for Melee, DEX for Ranged/Daggers, INT for Magic.
    /// </summary>
    public static int CalculateRawDamage(int baseWeaponDamage, int weaponScalingStat, bool isCrit)
    {
        double scaleBonus = weaponScalingStat * 0.5;
        double variance = RNGManager.RollDouble(0.9, 1.1);
        double critMultiplier = isCrit ? 1.5 : 1.0;

        double rawDamage = (baseWeaponDamage + scaleBonus) * variance * critMultiplier;
        return (int)Math.Round(rawDamage);
    }

    public static int CalculateFinalDamage(int rawDamage, int defenderArmor)
    {
        int finalDamage = rawDamage - defenderArmor;
        return Math.Max(1, finalDamage);
    }

    public static bool CalculateFleeSuccess(int fleeingDex, int fleeingCardio, int pursuerDex, int pursuerCardio)
    {
        return (fleeingDex + fleeingCardio) > (pursuerDex + pursuerCardio);
    }
}
