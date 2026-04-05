using System;
using System.Collections.Generic;

namespace DifferentWay.Systems;

public class MapNode
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public MapNodeType Type { get; set; }
}

public enum MapNodeType
{
    Kingdom,
    City,
    Village,
    POI // Point of Interest
}

public class Route
{
    public MapNode StartNode { get; set; } = null!;
    public MapNode EndNode { get; set; } = null!;
    public int DangerLevel { get; set; }
    public int WeightDistance { get; set; }
}

public class WorldTopology
{
    public List<MapNode> Nodes { get; private set; } = new();
    public List<Route> Routes { get; private set; } = new();

    private readonly Random _rnd = new Random();

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
