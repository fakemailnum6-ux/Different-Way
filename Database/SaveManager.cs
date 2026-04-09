using Godot;
using System.Text.Json;
using DifferentWay.Core;
using DifferentWay.Systems;

namespace DifferentWay.Database
{
    public partial class SaveManager : Node
    {
        private SQLiteConnector _db;

        public override void _Ready()
        {
             _db = GetNodeOrNull<SQLiteConnector>("/root/SQLiteConnector");
        }

        public void SaveGameState(GameState_Snapshot state)
        {
            if (_db == null) return;

            using var cmd = _db.CreateCommand();
            if (cmd == null) return;

            cmd.CommandText = @"
                INSERT OR REPLACE INTO PlayerState (id, location_id, health, max_health, gold, time)
                VALUES (1, @loc, @hp, @maxHp, @gold, '12:00');
            ";
            cmd.Parameters.AddWithValue("@loc", state.PlayerLocationId);
            cmd.Parameters.AddWithValue("@hp", state.PlayerHealth);
            cmd.Parameters.AddWithValue("@maxHp", state.PlayerMaxHealth);
            cmd.Parameters.AddWithValue("@gold", state.PlayerGold);

            cmd.ExecuteNonQuery();
            GD.Print("Saved GameState to SQLite");
        }

        public void SaveWorldNode(SettlementNode node)
        {
            if (_db == null) return;

            string json = JsonSerializer.Serialize(node);

            using var cmd = _db.CreateCommand();
            if (cmd == null) return;

            cmd.CommandText = @"
                INSERT OR REPLACE INTO WorldNodes (node_id, json_data)
                VALUES (@id, @json);
            ";
            cmd.Parameters.AddWithValue("@id", node.Id);
            cmd.Parameters.AddWithValue("@json", json);

            cmd.ExecuteNonQuery();
            GD.Print($"Saved SettlementNode {node.Id} to SQLite");
        }
    }
}
