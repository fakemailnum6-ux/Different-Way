using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Text.Json;
using Godot;
using DifferentWay.Systems;

namespace DifferentWay.Database;

public class SaveManager
{
    private SQLiteConnector? _dbConnector;

    public void SetupDirectories()
    {
        // 6.1. Пользовательские директории (Godot user://)
        var dirs = new string[] { "user://saves", "user://cache", "user://exported_worlds", "user://config" };

        foreach (var dir in dirs)
        {
            if (!DirAccess.DirExistsAbsolute(dir))
            {
                DirAccess.MakeDirAbsolute(dir);
            }
        }
    }

    public void LoadGame(string saveName)
    {
        SetupDirectories();
        _dbConnector = new SQLiteConnector(saveName);
        _dbConnector.InitializeDatabase();
    }

    public SQLiteConnector? GetDbConnector() => _dbConnector;

    public void SaveWorldTopology(WorldTopology topology)
    {
        string path = ProjectSettings.GlobalizePath("user://exported_worlds/macro_world.json");
        try
        {
            var dto = new WorldTopologyDto
            {
                Nodes = topology.Nodes,
                Routes = topology.Routes
            };

            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(dto, options);
            File.WriteAllText(path, json);
            DifferentWay.Core.GameLogger.Log("World topology exported to user://exported_worlds/");
        }
        catch (System.Exception ex)
        {
            DifferentWay.Core.GameLogger.LogError($"Failed to save world topology: {ex.Message}");
        }
    }

    public WorldTopology? LoadWorldTopology()
    {
        string path = ProjectSettings.GlobalizePath("user://exported_worlds/macro_world.json");
        if (!File.Exists(path)) return null;

        try
        {
            string json = File.ReadAllText(path);
            var dto = JsonSerializer.Deserialize<WorldTopologyDto>(json);
            if (dto != null)
            {
                var topology = new WorldTopology();
                topology.Nodes = dto.Nodes;
                topology.Routes = dto.Routes;
                return topology;
            }
            return null;
        }
        catch (System.Exception ex)
        {
            DifferentWay.Core.GameLogger.LogError($"Failed to load world topology: {ex.Message}");
            return null;
        }
    }

    // 6.3. Система Кэширования ИИ
    public string? GetCachedDialogue(string npcId, string stateContext)
    {
        if (_dbConnector == null) return null;

        string hash = GenerateHash(npcId + stateContext);

        using var conn = _dbConnector.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT cached_text FROM DialogueCache WHERE hash = @hash";
        cmd.Parameters.AddWithValue("@hash", hash);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return reader.GetString(0);
        }

        return null;
    }

    public void SaveCachedDialogue(string npcId, string stateContext, string response)
    {
        if (_dbConnector == null) return;

        string hash = GenerateHash(npcId + stateContext);

        using var conn = _dbConnector.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT OR REPLACE INTO DialogueCache (hash, cached_text) VALUES (@hash, @text)";
        cmd.Parameters.AddWithValue("@hash", hash);
        cmd.Parameters.AddWithValue("@text", response);
        cmd.ExecuteNonQuery();
    }

    private string GenerateHash(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha256.ComputeHash(bytes);
        return System.Convert.ToHexString(hash);
    }

    public void SaveGameState(DifferentWay.Core.GameState state)
    {
        if (_dbConnector == null) return;

        try
        {
            using var conn = _dbConnector.GetConnection();
            using var tx = conn.BeginTransaction();

            // 1. Save PlayerStats (Simplified for example)
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = @"
                    INSERT OR REPLACE INTO PlayerState (id, level, x_coord, y_coord, in_game_time)
                    VALUES (1, @level, @x, @y, @time)";
                cmd.Parameters.AddWithValue("@level", state.PlayerStats.STR); // Mock Level
                cmd.Parameters.AddWithValue("@x", 0.0); // Mock coordinates
                cmd.Parameters.AddWithValue("@y", 0.0);
                cmd.Parameters.AddWithValue("@time", 0.0);
                cmd.ExecuteNonQuery();
            }

            // 2. Save Inventory
            using (var cmdClear = conn.CreateCommand())
            {
                cmdClear.Transaction = tx;
                cmdClear.CommandText = "DELETE FROM Inventory WHERE player_id = 1";
                cmdClear.ExecuteNonQuery();
            }

            foreach (var item in state.PlayerInventory.Items)
            {
                using var cmdInv = conn.CreateCommand();
                cmdInv.Transaction = tx;
                cmdInv.CommandText = @"
                    INSERT INTO Inventory (player_id, item_id, quantity, current_durability)
                    VALUES (1, @itemId, @qty, @durability)";
                cmdInv.Parameters.AddWithValue("@itemId", item.Key);
                cmdInv.Parameters.AddWithValue("@qty", item.Value);
                cmdInv.Parameters.AddWithValue("@durability", 100);
                cmdInv.ExecuteNonQuery();
            }

            // 3. Save Gold (Storing as a special item in Inventory table)
            using (var cmdGold = conn.CreateCommand())
            {
                cmdGold.Transaction = tx;
                cmdGold.CommandText = @"
                    INSERT INTO Inventory (player_id, item_id, quantity, current_durability)
                    VALUES (1, 'gold', @qty, 100)";
                cmdGold.Parameters.AddWithValue("@qty", state.PlayerInventory.Gold);
                cmdGold.ExecuteNonQuery();
            }

            tx.Commit();
            DifferentWay.Core.GameLogger.Log("Игра успешно сохранена в базу данных.");
        }
        catch (System.Exception ex)
        {
            DifferentWay.Core.GameLogger.LogError($"Ошибка сохранения игры: {ex.Message}");
        }
    }

    public void LoadGameState(DifferentWay.Core.GameState state)
    {
        if (_dbConnector == null) return;

        try
        {
            using var conn = _dbConnector.GetConnection();

            // 1. Load PlayerStats
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT level, x_coord, y_coord FROM PlayerState WHERE id = 1";
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    state.PlayerStats.STR = reader.GetInt32(0); // Restoring mock Level
                    // Coordinates would be applied to Simulation here
                }
            }

            // 2. Load Inventory
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT item_id, quantity FROM Inventory WHERE player_id = 1";
                using var reader = cmd.ExecuteReader();

                state.PlayerInventory.ClearAllItems();
                state.PlayerInventory.Gold = 0;

                while (reader.Read())
                {
                    string itemId = reader.GetString(0);
                    int qty = reader.GetInt32(1);

                    if (itemId == "gold")
                    {
                        state.PlayerInventory.AddGold(qty);
                    }
                    else
                    {
                        state.PlayerInventory.AddItem(itemId, qty);
                    }
                }
            }

            DifferentWay.Core.GameLogger.Log("Игра успешно загружена из базы данных.");
        }
        catch (System.Exception ex)
        {
            DifferentWay.Core.GameLogger.LogError($"Ошибка загрузки игры: {ex.Message}");
        }
    }
}
