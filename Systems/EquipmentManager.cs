using System.Collections.Generic;
using Godot;
using DifferentWay.Systems.Models;

namespace DifferentWay.Systems;

public partial class EquipmentManager : RefCounted
{
    public WeaponData? EquippedWeapon { get; private set; }
    public ArmorData? EquippedArmor { get; private set; }

    public int GetTotalWeaponDamage()
    {
        return EquippedWeapon?.Damage ?? 5; // 5 is base unarmed damage
    }

    public int GetTotalArmorValue()
    {
        return EquippedArmor?.Armor ?? 0;
    }

    public bool EquipItem(string itemId, InventoryManager inventory)
    {
        if (inventory.GetItemCount(itemId) <= 0)
        {
            DifferentWay.Core.GameLogger.LogError($"Предмет {itemId} отсутствует в инвентаре.");
            return false;
        }

        // Check if it's a weapon
        if (DataManager.Weapons.TryGetValue(itemId, out var weaponData))
        {
            if (EquippedWeapon != null)
            {
                // Unequip old weapon back to inventory
                DifferentWay.Core.GameLogger.Log($"Оружие {EquippedWeapon.Name} снято.");
            }

            EquippedWeapon = weaponData;
            DifferentWay.Core.GameLogger.Log($"Экипировано оружие: {weaponData.Name} (Урон: {weaponData.Damage})");
            return true;
        }

        // Check if it's armor
        if (DataManager.Armors.TryGetValue(itemId, out var armorData))
        {
            if (EquippedArmor != null)
            {
                // Unequip old armor
                DifferentWay.Core.GameLogger.Log($"Броня {EquippedArmor.Name} снята.");
            }

            EquippedArmor = armorData;
            DifferentWay.Core.GameLogger.Log($"Экипирована броня: {armorData.Name} (Защита: {armorData.Armor})");
            return true;
        }

        DifferentWay.Core.GameLogger.Log($"Предмет {itemId} нельзя экипировать.");
        return false;
    }

    public void UnequipWeapon()
    {
        if (EquippedWeapon != null)
        {
            DifferentWay.Core.GameLogger.Log($"Оружие {EquippedWeapon.Name} снято.");
            EquippedWeapon = null;
        }
    }

    public void UnequipArmor()
    {
        if (EquippedArmor != null)
        {
            DifferentWay.Core.GameLogger.Log($"Броня {EquippedArmor.Name} снята.");
            EquippedArmor = null;
        }
    }

    // GDScript Getters
    public string GetEquippedWeaponName() => EquippedWeapon?.Name ?? "Безоружный";
    public string GetEquippedArmorName() => EquippedArmor?.Name ?? "Лохмотья";
}
