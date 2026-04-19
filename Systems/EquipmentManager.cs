using Godot;
using System;

public partial class EquipmentManager : RefCounted
{
    public WeaponData EquippedWeapon { get; private set; }
    public ArmorData EquippedArmor { get; private set; }

    public void EquipWeapon(string weaponId)
    {
        var dm = ServiceLocator.DataManager;
        if (dm != null && dm.Weapons.TryGetValue(weaponId, out var weapon))
        {
            EquippedWeapon = weapon;
            ServiceLocator.Logger.LogInfo($"EquipmentManager: Equipped {weapon.Name}.");
        }
        else
        {
            ServiceLocator.Logger.LogWarning($"EquipmentManager: Weapon {weaponId} not found in data compendium.");
        }
    }

    public void EquipArmor(string armorId)
    {
        var dm = ServiceLocator.DataManager;
        if (dm != null && dm.Armors.TryGetValue(armorId, out var armor))
        {
            EquippedArmor = armor;
            ServiceLocator.Logger.LogInfo($"EquipmentManager: Equipped {armor.Name}.");
        }
        else
        {
            ServiceLocator.Logger.LogWarning($"EquipmentManager: Armor {armorId} not found in data compendium.");
        }
    }

    public void UnequipWeapon()
    {
        EquippedWeapon = null;
        ServiceLocator.Logger.LogInfo("EquipmentManager: Unequipped weapon.");
    }

    public void UnequipArmor()
    {
        EquippedArmor = null;
        ServiceLocator.Logger.LogInfo("EquipmentManager: Unequipped armor.");
    }

    /// <summary>
    /// Calculates the current damage bonus based on durability.
    /// If durability reaches 0, stats are reduced to 1 (Section 4.3).
    /// </summary>
    public int GetTotalWeaponDamage()
    {
        if (EquippedWeapon == null) return 1; // Base fist damage

        if (EquippedWeapon.CurrentDurability <= 0)
        {
            ServiceLocator.Logger.LogWarning($"EquipmentManager: {EquippedWeapon.Name} is broken! Damage reduced to 1.");
            return 1;
        }

        return EquippedWeapon.Damage;
    }

    /// <summary>
    /// Calculates the current armor defense based on durability.
    /// If durability reaches 0, stats are reduced to 0 (Section 4.3).
    /// </summary>
    public int GetTotalArmorDefense()
    {
        if (EquippedArmor == null) return 0;

        if (EquippedArmor.CurrentDurability <= 0)
        {
            ServiceLocator.Logger.LogWarning($"EquipmentManager: {EquippedArmor.Name} is broken! Armor reduced to 0.");
            return 0;
        }

        return EquippedArmor.Armor;
    }

    public void DegradeWeapon(int amount = 1)
    {
        if (EquippedWeapon != null && EquippedWeapon.CurrentDurability > 0)
        {
            EquippedWeapon.CurrentDurability = Math.Max(0, EquippedWeapon.CurrentDurability - amount);
            if (EquippedWeapon.CurrentDurability == 0)
            {
                ServiceLocator.Logger.LogWarning($"EquipmentManager: {EquippedWeapon.Name} just broke!");
            }
        }
    }

    public void DegradeArmor(int amount = 1)
    {
        if (EquippedArmor != null && EquippedArmor.CurrentDurability > 0)
        {
            EquippedArmor.CurrentDurability = Math.Max(0, EquippedArmor.CurrentDurability - amount);
            if (EquippedArmor.CurrentDurability == 0)
            {
                ServiceLocator.Logger.LogWarning($"EquipmentManager: {EquippedArmor.Name} just broke!");
            }
        }
    }
}
