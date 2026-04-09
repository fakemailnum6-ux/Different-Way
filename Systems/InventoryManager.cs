using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class InventoryManager : RefCounted
{
    private readonly Dictionary<string, int> _items = new Dictionary<string, int>();

    /// <summary>
    /// Exposes read-only dictionary for SQLite serialization as noted in memory.
    /// </summary>
    public IReadOnlyDictionary<string, int> Items => _items;

    public void AddItem(string itemId, int quantity = 1)
    {
        if (quantity <= 0) return;

        if (_items.ContainsKey(itemId))
        {
            _items[itemId] += quantity;
        }
        else
        {
            _items[itemId] = quantity;
        }

        ServiceLocator.Logger?.LogInfo($"InventoryManager: Added {quantity}x {itemId}. Total: {_items[itemId]}");
    }

    public bool RemoveItem(string itemId, int quantity = 1)
    {
        if (!_items.ContainsKey(itemId) || _items[itemId] < quantity)
        {
            ServiceLocator.Logger?.LogWarning($"InventoryManager: Not enough {itemId} to remove.");
            return false;
        }

        _items[itemId] -= quantity;
        if (_items[itemId] <= 0)
        {
            _items.Remove(itemId);
        }

        ServiceLocator.Logger?.LogInfo($"InventoryManager: Removed {quantity}x {itemId}.");
        return true;
    }

    public int GetItemCount(string itemId)
    {
        return _items.TryGetValue(itemId, out int count) ? count : 0;
    }

    public float CalculateTotalWeight()
    {
        float totalWeight = 0f;
        var dm = ServiceLocator.DataManager;

        if (dm == null) return 0f;

        foreach (var kvp in _items)
        {
            string id = kvp.Key;
            int qty = kvp.Value;

            if (dm.Weapons.TryGetValue(id, out var weapon))
                totalWeight += weapon.Weight * qty;
            else if (dm.Armors.TryGetValue(id, out var armor))
                totalWeight += armor.Weight * qty;
            else if (dm.Consumables.TryGetValue(id, out var consumable))
                totalWeight += consumable.Weight * qty;
            else if (dm.Materials.TryGetValue(id, out var mat))
                totalWeight += mat.Weight * qty;
        }

        return totalWeight;
    }

    public void ClearAllItems()
    {
        _items.Clear();
        ServiceLocator.Logger?.LogInfo("InventoryManager: Inventory cleared.");
    }
}
