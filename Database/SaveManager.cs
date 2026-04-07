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
}
