using System;
using Godot;
using Microsoft.Data.Sqlite;

namespace DifferentWay.Database;

public class SQLiteConnector
{
    private string _connectionString;

    public SQLiteConnector(string saveName)
    {
        // 6.1, 6.2 user://saves/ — БД сохранений. Каждое сохранение — это файл .db.
        string savePath = ProjectSettings.GlobalizePath($"user://saves/{saveName}.db");
        _connectionString = $"Data Source={savePath}";
    }

    public void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS PlayerState (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                level INTEGER,
                x_coord REAL,
                y_coord REAL,
                in_game_time REAL
            );

            CREATE TABLE IF NOT EXISTS Inventory (
                player_id INTEGER,
                item_id TEXT,
                quantity INTEGER,
                current_durability INTEGER,
                PRIMARY KEY(player_id, item_id)
            );

            CREATE TABLE IF NOT EXISTS WorldNodes (
                node_id TEXT PRIMARY KEY,
                is_unlocked INTEGER
            );

            CREATE TABLE IF NOT EXISTS EntitiesState (
                entity_id TEXT PRIMARY KEY,
                is_dead INTEGER,
                npc_state TEXT
            );

            CREATE TABLE IF NOT EXISTS QuestProgress (
                quest_id TEXT PRIMARY KEY,
                state_json TEXT
            );

            CREATE TABLE IF NOT EXISTS DialogueCache (
                hash TEXT PRIMARY KEY,
                cached_text TEXT
            );

            CREATE TABLE IF NOT EXISTS Memories (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                timestamp TEXT,
                keywords TEXT,
                content TEXT
            );
        ";
        command.ExecuteNonQuery();

        GD.Print("Database schema initialized.");
    }

    public SqliteConnection GetConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }
}
