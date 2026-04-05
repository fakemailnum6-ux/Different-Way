using System;
using System.Text.Json;
using System.IO;
using Godot;

namespace DifferentWay.Database
{
    public class SaveData
    {
        public string SaveVersion { get; set; } = "1.0";
        public int WorldSeed { get; set; }
        public Core.GameState GameState { get; set; }
    }

    public class SaveManager
    {
        private readonly string _saveFilePath = "user_save.json";

        public void Save(SaveData data)
        {
            try
            {
                string jsonString = JsonSerializer.Serialize(data);
                File.WriteAllText(_saveFilePath, jsonString);
                GD.Print($"Game saved successfully to {_saveFilePath}");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Failed to save game: {ex.Message}");
            }
        }

        public SaveData Load()
        {
            if (!File.Exists(_saveFilePath))
            {
                GD.Print("No save file found. Starting new game.");
                return null;
            }

            try
            {
                string jsonString = File.ReadAllText(_saveFilePath);
                SaveData data = JsonSerializer.Deserialize<SaveData>(jsonString);

                Migrate(data);

                GD.Print("Game loaded successfully.");
                return data;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Failed to load game: {ex.Message}");
                return null;
            }
        }

        private void Migrate(SaveData data)
        {
            // Stub for migration logic
            if (data.SaveVersion != "2.0")
            {
                GD.Print($"Migrating save from {data.SaveVersion} to 2.0");
                data.SaveVersion = "2.0";
            }
        }
    }
}
