namespace DifferentWay.Systems;

public static class DamageCalculator
{
    public static int CalculateDamage(int baseDamage, int targetArmor)
    {
        int damage = baseDamage - targetArmor;
        return damage > 0 ? damage : 0;
    }
}
