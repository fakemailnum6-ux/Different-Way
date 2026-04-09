using System.Collections.Generic;
using Godot;

namespace DifferentWay.Systems
{
    // The overarching kingdom region
    public class KingdomNode
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<SettlementNode> Settlements { get; set; } = new List<SettlementNode>();
    }

    // A specific town/village (LocalMap level)
    public class SettlementNode
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Biome { get; set; }
        public List<BuildingNode> Buildings { get; set; } = new List<BuildingNode>();
        // Simple adjacency list for local roads between buildings
        public Dictionary<string, List<string>> Roads { get; set; } = new Dictionary<string, List<string>>();
    }

    // A specific POI inside a settlement (Interaction Menu level)
    public class BuildingNode
    {
        public string Id { get; set; }
        public string Type { get; set; } // Tavern, Smithy, Farm
        public Vector2 Position { get; set; } // For drawing on LocalMap
        public List<NpcNode> Npcs { get; set; } = new List<NpcNode>();
    }

    // Characters residing in buildings
    public class NpcNode
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string GoapGoal { get; set; }
        // public CharacterStats Stats { get; set; } = new CharacterStats(); // Omitted for now, relying on stub
        // public Dictionary<string, int> Inventory { get; set; } = new Dictionary<string, int>(); // Omitted
    }
}
