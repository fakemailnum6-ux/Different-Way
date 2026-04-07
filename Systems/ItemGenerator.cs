using System;
using System.Collections.Generic;

namespace DifferentWay.Systems;

public static class ItemGenerator
{
    private static readonly Random _rnd = new Random();

    public static Dictionary<string, int> GenerateLootForMob(string mobId, int playerLuck)
    {
        var loot = new Dictionary<string, int>();

        if (!DataManager.LootTables.TryGetValue(mobId, out var table))
        {
            return loot;
        }

        // Arc.md: Удача (Luck) -> Улучшение генерации лута. (e.g. +Luck/2 % drop chance)
        int luckBonus = playerLuck / 2;

        foreach (var drop in table.Drops)
        {
            int roll = _rnd.Next(1, 101);
            int finalChance = drop.Chance + luckBonus;

            if (roll <= finalChance)
            {
                int quantity = _rnd.Next(drop.MinCount, drop.MaxCount + 1);
                loot[drop.ItemId] = quantity;
            }
        }

        return loot;
    }
}
