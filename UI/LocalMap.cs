using Godot;
using System.Collections.Generic;

public partial class LocalMap : Control
{
    private readonly List<Vector2> _buildingPositions = new List<Vector2>();
    private readonly List<(int startIndex, int endIndex)> _roads = new List<(int, int)>();

    public override void _Ready()
    {
        // For Code-Only UI, ensure this control covers the screen
        SetAnchorsPreset(LayoutPreset.FullRect);
    }

    /// <summary>
    /// Loads a generated map node structure and visualizes it.
    /// </summary>
    public void LoadMap(WorldNodeDto mapData)
    {
        ClearMap();

        // Normally, TopologyJson from mapData would be parsed here.
        // For demonstration of the Code-Only rendering constraint:
        _buildingPositions.Add(new Vector2(200, 200)); // Tavern
        _buildingPositions.Add(new Vector2(600, 250)); // Blacksmith
        _buildingPositions.Add(new Vector2(400, 500)); // Player House

        _roads.Add((0, 1));
        _roads.Add((0, 2));
        _roads.Add((1, 2));

        SpawnBuildingRects();
        QueueRedraw(); // Trigger _Draw() for lines
    }

    private void SpawnBuildingRects()
    {
        foreach (var pos in _buildingPositions)
        {
            ColorRect rect = new ColorRect
            {
                Color = new Color(0.2f, 0.6f, 0.8f, 1f), // Blue-ish buildings
                Position = new Vector2(pos.X - 25, pos.Y - 25), // Center rect
                Size = new Vector2(50, 50)
            };

            // Hook up interaction signal
            rect.GuiInput += (InputEvent e) =>
            {
                if (e is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
                {
                    OnBuildingClicked(rect);
                }
            };

            AddChild(rect);
        }
    }

    public override void _Draw()
    {
        // Draw roads underneath the buildings
        foreach (var road in _roads)
        {
            Vector2 start = _buildingPositions[road.startIndex];
            Vector2 end = _buildingPositions[road.endIndex];

            DrawLine(start, end, Colors.SaddleBrown, width: 4.0f, antialiased: true);
        }
    }

    private void OnBuildingClicked(ColorRect source)
    {
        GD.Print($"[LocalMap] Clicked building at {source.Position}");
        // Transition to interaction node state
        // ServiceLocator.EventBus.EmitSignal(EventBus.SignalName.RequestStateChange, (int)GameState.Dialogue);
    }

    public void ClearMap()
    {
        foreach (Node child in GetChildren())
        {
            child.QueueFree();
        }
        _buildingPositions.Clear();
        _roads.Clear();
        QueueRedraw();
    }
}
