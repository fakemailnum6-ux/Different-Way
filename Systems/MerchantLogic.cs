using System;
using Godot;
using DifferentWay.Systems.Models;

namespace DifferentWay.Systems;

public partial class MerchantLogic : RefCounted
{
    public int CalculateBuyPrice(int basePrice, int playerCharisma)
    {
        // 10.1 Arc.md: Харизма (Скидки)
        // High charisma lowers the price you pay (e.g. 10 Charisma = 5% discount)
        float discountPercent = Math.Clamp(playerCharisma * 0.5f, 0, 50);
        float multiplier = (100f - discountPercent) / 100f;
        return (int)Math.Max(1, basePrice * multiplier);
    }

    public int CalculateSellPrice(int basePrice, int playerCharisma)
    {
        // Selling items usually yields less than buying, but Charisma improves it
        float markupPercent = Math.Clamp(playerCharisma * 0.5f, 0, 50);
        float multiplier = (30f + markupPercent) / 100f; // Baseline 30% of original price
        return (int)Math.Max(1, basePrice * multiplier);
    }

    public int GetItemBasePrice(string itemId)
    {
        if (DataManager.Weapons.TryGetValue(itemId, out var weapon)) return weapon.Price;
        if (DataManager.Armors.TryGetValue(itemId, out var armor)) return armor.Price;

        // Materials/Consumables base prices (mocked if not defined in Models)
        // Consumables usually cost around 10-50 in RPGs
        if (DataManager.Consumables.ContainsKey(itemId)) return 20;
        if (DataManager.Materials.ContainsKey(itemId)) return 5;

        return 1;
    }
}
