using System;
using System.Collections.Generic;
using Godot;

namespace DifferentWay.Systems;

public enum TopologyLevel
{
    Kingdom = 0,
    City = 1,
    Village = 2,
    POI = 3
}

public partial class MapNode : RefCounted
{
    public string id { get; set; } = string.Empty;
    public string name { get; set; } = string.Empty;
    public TopologyLevel Level { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public bool IsUnlocked { get; set; }
}

public partial class Route : RefCounted
{
    public string StartNodeId { get; set; } = string.Empty;
    public string EndNodeId { get; set; } = string.Empty;
    public int DangerLevel { get; set; }
    public int WeightDistance { get; set; }
}

public partial class WorldTopology : RefCounted
{
    public List<MapNode> Nodes { get; private set; } = new();
    public List<Route> Routes { get; private set; } = new();

    private readonly Random _rnd = new Random();

    public void GenerateMacroWorld()
    {
        Nodes.Clear();
        Routes.Clear();

        // Kingdom Center
        var kingdom = new MapNode { id = "kingdom_1", name = "Этерия", Level = TopologyLevel.Kingdom, X = 500, Y = 500, IsUnlocked = true };

        // Cities
        var city1 = new MapNode { id = "city_1", name = "Северный Предел", Level = TopologyLevel.City, X = 500, Y = 200, IsUnlocked = false };
        var city2 = new MapNode { id = "city_2", name = "Торговая Гавань", Level = TopologyLevel.City, X = 800, Y = 500, IsUnlocked = false };

        // Villages/POI
        var village1 = new MapNode { id = "village_start", name = "Дубовая Гавань", Level = TopologyLevel.Village, X = 800, Y = 800, IsUnlocked = true };
        var village2 = new MapNode { id = "village_2", name = "Рыбацкая Деревня", Level = TopologyLevel.Village, X = 200, Y = 500, IsUnlocked = false };
        var poi1 = new MapNode { id = "poi_1", name = "Заброшенная Шахта", Level = TopologyLevel.POI, X = 650, Y = 350, IsUnlocked = false };

        Nodes.Add(kingdom);
        Nodes.Add(city1);
        Nodes.Add(city2);
        Nodes.Add(village1);
        Nodes.Add(village2);
        Nodes.Add(poi1);

        // Procedural Edges
        Routes.Add(new Route { StartNodeId = kingdom.id, EndNodeId = city1.id, DangerLevel = 2, WeightDistance = 300 });
        Routes.Add(new Route { StartNodeId = kingdom.id, EndNodeId = city2.id, DangerLevel = 1, WeightDistance = 300 });
        Routes.Add(new Route { StartNodeId = kingdom.id, EndNodeId = village2.id, DangerLevel = 4, WeightDistance = 300 });
        Routes.Add(new Route { StartNodeId = city2.id, EndNodeId = village1.id, DangerLevel = 3, WeightDistance = 300 });
        Routes.Add(new Route { StartNodeId = city1.id, EndNodeId = poi1.id, DangerLevel = 6, WeightDistance = 150 });

        DifferentWay.Core.GameLogger.Log("Сгенерирована структура Макро-Мира (граф поселений).");
    }

    public Godot.Collections.Array<MapNode> GetUnlockedVillages()
    {
        var result = new Godot.Collections.Array<MapNode>();
        foreach (var node in Nodes)
        {
            if (node.IsUnlocked) result.Add(node);
        }
        return result;
    }

    public Godot.Collections.Array<MapNode> GetAllNodes()
    {
        var result = new Godot.Collections.Array<MapNode>();
        foreach (var n in Nodes) result.Add(n);
        return result;
    }

    public Godot.Collections.Array<Route> GetAllRoutes()
    {
        var result = new Godot.Collections.Array<Route>();
        foreach (var r in Routes) result.Add(r);
        return result;
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
