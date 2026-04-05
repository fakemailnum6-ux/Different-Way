using System;
using Godot;
using Microsoft.Data.Sqlite;
using System.Text.Json;

public class SaveManager
{
    private readonly SQLiteConnector _dbConnector;
    private const int CURRENT_SAVE_VERSION = 1;

    public SaveManager(SQLiteConnector dbConnector)
    {
        _dbConnector = dbConnector;
        GD.Print("[Database] SaveManager initialized.");
    }

    public void SaveGame(string saveName, int worldSeed, object gameStateData)
    {
        try
        {
            string jsonData = JsonSerializer.Serialize(gameStateData);

            using (var connection = _dbConnector.GetConnection())
            {
                connection.Open();

                string insertQuery = @"
                    INSERT INTO Saves (SaveName, SaveVersion, WorldSeed, SaveData)
                    VALUES (@SaveName, @SaveVersion, @WorldSeed, @SaveData);
                ";

                using (var command = new SqliteCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@SaveName", saveName);
                    command.Parameters.AddWithValue("@SaveVersion", CURRENT_SAVE_VERSION);
                    command.Parameters.AddWithValue("@WorldSeed", worldSeed);
                    command.Parameters.AddWithValue("@SaveData", jsonData);

                    command.ExecuteNonQuery();
                }
            }

            GD.Print($"[Database] Successfully saved game '{saveName}'.");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[Database] Failed to save game: {ex.Message}");
        }
    }

    public string LoadGame(string saveName)
    {
        try
        {
            using (var connection = _dbConnector.GetConnection())
            {
                connection.Open();

                string selectQuery = @"
                    SELECT SaveVersion, WorldSeed, SaveData FROM Saves
                    WHERE SaveName = @SaveName
                    ORDER BY Timestamp DESC LIMIT 1;
                ";

                using (var command = new SqliteCommand(selectQuery, connection))
                {
                    command.Parameters.AddWithValue("@SaveName", saveName);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int saveVersion = reader.GetInt32(0);
                            int worldSeed = reader.GetInt32(1);
                            string saveData = reader.GetString(2);

                            if (saveVersion < CURRENT_SAVE_VERSION)
                            {
                                saveData = RunMigrations(saveData, saveVersion, CURRENT_SAVE_VERSION);
                            }

                            GD.Print($"[Database] Successfully loaded game '{saveName}'.");
                            return saveData;
                        }
                    }
                }
            }

            GD.PrintErr($"[Database] Save '{saveName}' not found.");
            return null;
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[Database] Failed to load game: {ex.Message}");
            return null;
        }
    }

    private string RunMigrations(string saveData, int oldVersion, int newVersion)
    {
        GD.Print($"[Database] Running migrations from version {oldVersion} to {newVersion}...");

        // Placeholder for migration logic
        // In a real scenario, we would parse JSON, add missing fields with defaults, then reserialize.

        return saveData;
    }
}
