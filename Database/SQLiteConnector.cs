using System;
using Microsoft.Data.Sqlite;
using Godot;

namespace DifferentWay.Database
{
    public class SQLiteConnector
    {
        private readonly string _connectionString = "Data Source=data.db";

        public SQLiteConnector()
        {
        }

        public void Initialize()
        {
            try
            {
                using (var connection = new SqliteConnection(_connectionString))
                {
                    connection.Open();
                    GD.Print("SQLite connection opened successfully.");

                    // Basic table creation
                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Settings (
                            Key TEXT PRIMARY KEY,
                            Value TEXT NOT NULL
                        );
                        CREATE TABLE IF NOT EXISTS NPC_States (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Name TEXT NOT NULL,
                            StateData TEXT NOT NULL
                        );
                    ";
                    command.ExecuteNonQuery();
                    GD.Print("SQLite tables initialized.");
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Failed to initialize SQLite: {ex.Message}");
            }
        }

        public SqliteConnection GetConnection()
        {
            return new SqliteConnection(_connectionString);
        }
    }
}
