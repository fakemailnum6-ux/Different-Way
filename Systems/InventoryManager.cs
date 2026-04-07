using System.Collections.Generic;
using Godot;

namespace DifferentWay.Systems;

public partial class InventoryManager : RefCounted
{
    private Dictionary<string, int> _items = new();
    public IReadOnlyDictionary<string, int> Items => _items;
    public int Gold { get; set; } = 100;

    public void AddItem(string itemId, int amount)
    {
        if (amount <= 0) return;

        if (_items.ContainsKey(itemId))
            _items[itemId] += amount;
        else
            _items[itemId] = amount;

        DifferentWay.Core.GameLogger.Log($"Получен предмет: {itemId} x{amount}");
    }

    public bool RemoveItem(string itemId, int amount)
    {
        if (!_items.ContainsKey(itemId) || _items[itemId] < amount)
            return false;

        _items[itemId] -= amount;
        if (_items[itemId] <= 0)
        {
            _items.Remove(itemId);
        }

        DifferentWay.Core.GameLogger.Log($"Потерян предмет: {itemId} x{amount}");
        return true;
    }

    public int GetItemCount(string itemId)
    {
        return _items.TryGetValue(itemId, out int count) ? count : 0;
    }

    public bool HasItems(Dictionary<string, int> requirements)
    {
        foreach (var req in requirements)
        {
            if (GetItemCount(req.Key) < req.Value)
            {
                return false;
            }
        }
        return true;
    }

    // GDScript wrapper
    public Godot.Collections.Dictionary GetInventoryData()
    {
        var dict = new Godot.Collections.Dictionary();
        foreach (var kvp in _items)
        {
            dict[kvp.Key] = kvp.Value;
        }
        return dict;
    }

    public int GetGold() => Gold;
    public void AddGold(int amount) => Gold += amount;
    public void RemoveGold(int amount) => Gold -= amount;

    public void ClearAllItems()
    {
        _items.Clear();
    }
}
