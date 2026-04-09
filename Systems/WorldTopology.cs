using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Wraps Godot's AStar2D for string-based graph navigation of the macro/micro world.
/// Used to calculate paths between WorldNodes (Kingdoms, Cities, POIs).
/// </summary>
public partial class WorldTopology : RefCounted
{
    private AStar2D _aStar;
    private Dictionary<string, long> _stringToIdMap;
    private Dictionary<long, string> _idToStringMap;
    private long _nextId = 1;

    public WorldTopology()
    {
        _aStar = new AStar2D();
        _stringToIdMap = new Dictionary<string, long>();
        _idToStringMap = new Dictionary<long, string>();
    }

    /// <summary>
    /// Adds a coordinate point to the pathfinding graph representing a settlement or building.
    /// </summary>
    public void AddNode(string nodeId, Vector2 position)
    {
        if (_stringToIdMap.ContainsKey(nodeId))
        {
            ServiceLocator.Logger?.LogWarning($"WorldTopology: Node {nodeId} already exists. Updating position.");
            long id = _stringToIdMap[nodeId];
            _aStar.SetPointPosition(id, position);
            return;
        }

        long newId = _nextId++;
        _stringToIdMap[nodeId] = newId;
        _idToStringMap[newId] = nodeId;

        _aStar.AddPoint(newId, position);
    }

    /// <summary>
    /// Connects two nodes with an optional weight (e.g. higher weight = slower travel on a bad road).
    /// </summary>
    public void ConnectNodes(string nodeA, string nodeB, float weight = 1.0f, bool bidirectional = true)
    {
        if (!_stringToIdMap.TryGetValue(nodeA, out long idA) || !_stringToIdMap.TryGetValue(nodeB, out long idB))
        {
            ServiceLocator.Logger?.LogError($"WorldTopology: Cannot connect {nodeA} and {nodeB}. One or both nodes don't exist.");
            return;
        }

        _aStar.ConnectPoints(idA, idB, bidirectional);

        // Godot AStar2D sets weights per point, but we can set weight scale on the destination point to simulate edge cost
        _aStar.SetPointWeightScale(idB, weight);
        if (bidirectional)
        {
            _aStar.SetPointWeightScale(idA, weight);
        }
    }

    /// <summary>
    /// Finds the shortest path across the graph.
    /// Returns a list of node IDs. Returns empty if no path is found.
    /// </summary>
    public List<string> GetPath(string startNode, string endNode)
    {
        if (!_stringToIdMap.TryGetValue(startNode, out long startId) || !_stringToIdMap.TryGetValue(endNode, out long endId))
        {
            return new List<string>();
        }

        long[] pathIds = _aStar.GetIdPath(startId, endId);

        return pathIds.Select(id => _idToStringMap[id]).ToList();
    }

    /// <summary>
    /// Clear all nodes from the graph (useful when generating a new LocalMap).
    /// </summary>
    public void Clear()
    {
        _aStar.Clear();
        _stringToIdMap.Clear();
        _idToStringMap.Clear();
        _nextId = 1;
    }

    public Vector2 GetNodePosition(string nodeId)
    {
        if (_stringToIdMap.TryGetValue(nodeId, out long id))
        {
            return _aStar.GetPointPosition(id);
        }
        return Vector2.Zero;
    }
}
