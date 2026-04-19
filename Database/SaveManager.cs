using Godot;
using System;
using System.Linq;

public partial class SaveManager : RefCounted
{
    private SQLiteConnector _dbConnector;

    public SaveManager()
    {
        _dbConnector = new SQLiteConnector();
        _dbConnector.Connect();
    }

    public void SaveGameState(GameState_Live liveState)
    {
        var db = _dbConnector.GetConnection();
        if (db == null) return;

        try
        {
            db.RunInTransaction(() =>
            {
                // 1. Save Player State
                var playerDto = new PlayerStateDto
                {
                    PlayerId = 1, // Single-player, slot 1
                    CurrentHP = liveState.PlayerHP,
                    Gold = liveState.PlayerGold
                    // Other fields would be populated from StatsManager
                };

                // InsertOrReplace handles updating an existing row or creating a new one
                db.InsertOrReplace(playerDto);

                // 2. Save Inventory
                // First wipe existing inventory for this player to prevent orphaned items
                db.Execute("DELETE FROM InventoryItemDto WHERE PlayerId = ?", 1);

                var inventoryDtos = liveState.Inventory.Select(itemId => new InventoryItemDto
                {
                    PlayerId = 1,
                    ItemId = itemId,
                    Quantity = 1 // In a real inventory manager, this would pull the true quantity
                });

                db.InsertAll(inventoryDtos);

                ServiceLocator.Logger.LogInfo("SaveManager: Auto-save completed successfully.");
            });
        }
        catch (Exception ex)
        {
            ServiceLocator.Logger.LogError($"SaveManager: Failed to save game state: {ex.Message}");
        }
    }

    public void LoadGameState(GameState_Live liveState)
    {
        var db = _dbConnector.GetConnection();
        if (db == null) return;

        try
        {
            var playerDto = db.Table<PlayerStateDto>().FirstOrDefault(p => p.PlayerId == 1);
            if (playerDto != null)
            {
                liveState.PlayerHP = playerDto.CurrentHP;
                liveState.PlayerGold = playerDto.Gold;
            }

            var inventoryDtos = db.Table<InventoryItemDto>().Where(i => i.PlayerId == 1).ToList();
            liveState.Inventory.Clear();
            foreach (var item in inventoryDtos)
            {
                // Reconstruct inventory lists
                for(int i = 0; i < item.Quantity; i++)
                {
                    liveState.Inventory.Add(item.ItemId);
                }
            }

            ServiceLocator.Logger.LogInfo("SaveManager: Game state loaded from disk.");
        }
        catch (Exception ex)
        {
            ServiceLocator.Logger.LogError($"SaveManager: Failed to load game state: {ex.Message}");
        }
    }

    /// <summary>
    /// Arc.md Section 6.3: Dialogue Cache checks
    /// </summary>
    public string CheckDialogueCache(string npcId, string stateHash)
    {
        var db = _dbConnector.GetConnection();
        string key = $"{npcId}_{stateHash}";

        var cached = db?.Table<DialogueCacheDto>().FirstOrDefault(c => c.HashKey == key);
        return cached?.JsonResponse;
    }

    public void SaveDialogueCache(string npcId, string stateHash, string responseJson)
    {
        var db = _dbConnector.GetConnection();
        string key = $"{npcId}_{stateHash}";

        var dto = new DialogueCacheDto
        {
            HashKey = key,
            JsonResponse = responseJson
        };

        db?.InsertOrReplace(dto);
    }

    public void CloseConnection()
    {
        _dbConnector?.Close();
    }
}
