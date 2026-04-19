using Godot;
using Microsoft.Data.Sqlite;
using System;

namespace DifferentWay.Database
{
    public partial class SQLiteConnector : Node
    {
        private string _dbPath;
        private SqliteConnection _connection;

        public override void _Ready()
        {
             // Ensure directory exists
            var dir = Godot.DirAccess.Open("user://");
            if (!dir.DirExists("saves"))
            {
                dir.MakeDir("saves");
            }

            // Resolve godot user path to absolute OS path
            string globalPath = ProjectSettings.GlobalizePath("user://saves/savegame.db");
            _dbPath = $"Data Source={globalPath}";

            Connect();
        }

        public void Connect()
        {
            try
            {
                _connection = new SqliteConnection(_dbPath);
                _connection.Open();
                GD.Print($"Connected to SQLite at {_dbPath}");
                InitializeSchema();
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Failed to connect to SQLite: {ex.Message}");
            }
        }

        private void InitializeSchema()
        {
            if (_connection == null || _connection.State != System.Data.ConnectionState.Open) return;

            string createPlayerState = @"
                CREATE TABLE IF NOT EXISTS PlayerState (
                    id INTEGER PRIMARY KEY,
                    location_id TEXT,
                    health INTEGER,
                    max_health INTEGER,
                    gold INTEGER,
                    time TEXT
                );
            ";
            ExecuteNonQuery(createPlayerState);

            string createInventory = @"
                CREATE TABLE IF NOT EXISTS Inventory (
                    item_id TEXT PRIMARY KEY,
                    quantity INTEGER,
                    current_durability INTEGER
                );
            ";
            ExecuteNonQuery(createInventory);

            string createWorldNodes = @"
                CREATE TABLE IF NOT EXISTS WorldNodes (
                    node_id TEXT PRIMARY KEY,
                    json_data TEXT
                );
            ";
            ExecuteNonQuery(createWorldNodes);

            GD.Print("SQLite Schema Initialized.");
        }

        public void Disconnect()
        {
            if (_connection != null && _connection.State == System.Data.ConnectionState.Open)
            {
                _connection.Close();
            }
        }

        public SqliteCommand CreateCommand()
        {
            if (_connection == null || _connection.State != System.Data.ConnectionState.Open) return null;
            return _connection.CreateCommand();
        }

        public int ExecuteNonQuery(string query)
        {
            if (_connection == null || _connection.State != System.Data.ConnectionState.Open) return 0;

            using var command = _connection.CreateCommand();
            command.CommandText = query;
            return command.ExecuteNonQuery();
        }

        public override void _ExitTree()
        {
            Disconnect();
        }
    }
}
