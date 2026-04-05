using System;
using System.Collections.Generic;
using Godot;

namespace DifferentWay.Systems;

public partial class MapNode : RefCounted
{
    public string id { get; set; } = string.Empty;
    public string name { get; set; } = string.Empty;
    public int Type { get; set; }
}

public class Route
{
    public MapNode StartNode { get; set; } = null!;
    public MapNode EndNode { get; set; } = null!;
    public int DangerLevel { get; set; }
    public int WeightDistance { get; set; }
}

public partial class WorldTopology : RefCounted
{
    public List<MapNode> Nodes { get; private set; } = new();
    public List<Route> Routes { get; private set; } = new();

    private readonly Random _rnd = new Random();

    public Godot.Collections.Array<MapNode> GetUnlockedVillages()
    {
        // Placeholder returning empty for GDScript interop testing
        return new Godot.Collections.Array<MapNode>();
    }

    public bool RollRandomEncounter(Route route, int playerLuck)
    {
        // Random encounters based on Danger and Player Luck
        // As defined in 4.4: World is a weighted graph. Dice roll on routes based on Danger and Luck.
        int baseChance = route.DangerLevel * 10;
        int mitigatedChance = Math.Max(0, baseChance - (playerLuck * 2));

        int roll = _rnd.Next(1, 101);
        return roll <= mitigatedChance;
    }
}
