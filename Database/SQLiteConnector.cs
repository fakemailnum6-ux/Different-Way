using System;
using System.IO;
using Godot;
using Microsoft.Data.Sqlite;

public class SQLiteConnector
{
    private readonly string _dbPath;
    private readonly string _connectionString;

    public SQLiteConnector()
    {
        // For Godot, user:// is mapped to the OS user data folder
        // For testing/development, we might just put it in a local directory or Godot's ProjectSettings.GlobalizePath("user://save.db")

        string userDataDir = ProjectSettings.GlobalizePath("user://");
        if (string.IsNullOrEmpty(userDataDir))
        {
            // fallback if not running inside Godot main loop (like in tests)
            userDataDir = Directory.GetCurrentDirectory();
        }

        _dbPath = Path.Combine(userDataDir, "database.sqlite");
        _connectionString = $"Data Source={_dbPath}";

        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        try
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                // Create basic tables for saves if they don't exist
                string createSavesTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Saves (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        SaveName TEXT NOT NULL,
                        SaveVersion INTEGER NOT NULL,
                        WorldSeed INTEGER NOT NULL,
                        Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
                        SaveData TEXT NOT NULL
                    );
                ";

                using (var command = new SqliteCommand(createSavesTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }

                // E.g. other tables...
                string createDialogsTableQuery = @"
                    CREATE TABLE IF NOT EXISTS FallbackDialogs (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        StateKey TEXT UNIQUE NOT NULL,
                        FallbackText TEXT NOT NULL
                    );
                ";

                using (var command = new SqliteCommand(createDialogsTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }

                GD.Print($"[Database] SQLite initialized at: {_dbPath}");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[Database] Failed to initialize SQLite database: {ex.Message}");
        }
    }

    public SqliteConnection GetConnection()
    {
        return new SqliteConnection(_connectionString);
    }
}
