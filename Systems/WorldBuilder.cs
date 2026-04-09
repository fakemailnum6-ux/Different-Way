using Godot;
using System;
using System.Collections.Generic;

namespace DifferentWay.Systems
{
    public class LocationParams
    {
        public string Biome { get; set; }
        public string SettlementType { get; set; }
        public string Faction { get; set; }
        public int PlayerLevel { get; set; }
        public string NarrativeContext { get; set; }
    }

    public partial class WorldBuilder : Node
    {
        public override void _Ready()
        {
        }

        // Simulates the macro-generation pipeline.
        // In full implementation, this calls AI. For now, it returns a deterministic stub structure.
        public SettlementNode GenerateLocation(LocationParams p)
        {
            var settlement = new SettlementNode
            {
                Id = "zero_state_town",
                Name = "Забытый Перекресток",
                Description = "Маленькая деревня, сгенерированная на основе предыстории.",
                Biome = p.Biome
            };

            // Generate Buildings
            var tavern = new BuildingNode { Id = "b1", Type = "Tavern", Position = new Vector2(100, 100) };
            var smithy = new BuildingNode { Id = "b2", Type = "Smithy", Position = new Vector2(300, 150) };
            var farm = new BuildingNode { Id = "b3", Type = "Farm", Position = new Vector2(200, 300) };

            settlement.Buildings.Add(tavern);
            settlement.Buildings.Add(smithy);
            settlement.Buildings.Add(farm);

            // Algorithmic Topology: Connect buildings with roads (A to B, B to C)
            settlement.Roads["b1"] = new List<string> { "b2", "b3" };
            settlement.Roads["b2"] = new List<string> { "b1" };
            settlement.Roads["b3"] = new List<string> { "b1" };

            // Generate NPCs
            tavern.Npcs.Add(new NpcNode
            {
                Id = "npc_1",
                Name = "Трактирщик Боб",
                Role = "Merchant",
                GoapGoal = "Wait for customers"
            });

            return settlement;
        }
    }
}
