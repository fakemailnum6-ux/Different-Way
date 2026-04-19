using Godot;
using System.Collections.Generic;
using DifferentWay.Systems;

namespace DifferentWay.UI
{
    public partial class LocalMap : Control
    {
        private SettlementNode _currentSettlement;
        private Dictionary<string, Vector2> _buildingPositions = new Dictionary<string, Vector2>();

        public override void _Ready()
        {
            SetAnchorsPreset(LayoutPreset.FullRect);

            // Background
            var bg = new ColorRect { Color = new Color(0.1f, 0.1f, 0.1f) };
            bg.SetAnchorsPreset(LayoutPreset.FullRect);
            AddChild(bg);
        }

        public void LoadSettlement(SettlementNode settlement)
        {
            _currentSettlement = settlement;
            _buildingPositions.Clear();

            // Clear old children (except background)
            foreach (Node child in GetChildren())
            {
                if (child is ColorRect c && c.Color == new Color(0.1f, 0.1f, 0.1f)) continue; // Keep background
                child.QueueFree();
            }

            if (_currentSettlement == null) return;

            // First pass: cache positions
            foreach (var b in _currentSettlement.Buildings)
            {
                _buildingPositions[b.Id] = b.Position;
            }

            // Draw Roads (Lines)
            foreach (var kvp in _currentSettlement.Roads)
            {
                string fromId = kvp.Key;
                if (!_buildingPositions.ContainsKey(fromId)) continue;

                Vector2 fromPos = _buildingPositions[fromId];

                foreach (string toId in kvp.Value)
                {
                    if (!_buildingPositions.ContainsKey(toId)) continue;
                    Vector2 toPos = _buildingPositions[toId];

                    var line = new Line2D
                    {
                        DefaultColor = new Color(0.4f, 0.4f, 0.4f),
                        Width = 4f
                    };
                    line.AddPoint(fromPos);
                    line.AddPoint(toPos);
                    AddChild(line);
                }
            }

            // Draw Buildings (Rectangles)
            foreach (var b in _currentSettlement.Buildings)
            {
                var btn = new Button
                {
                    Text = b.Type,
                    Position = b.Position - new Vector2(40, 20), // Center the button somewhat
                    CustomMinimumSize = new Vector2(80, 40)
                };

                // Add click handler to enter building
                btn.Pressed += () => OnBuildingClicked(b.Id);

                AddChild(btn);
            }
        }

        private void OnBuildingClicked(string buildingId)
        {
            GD.Print($"Entered building: {buildingId}");
            // Future: Open InteractionMenuUI or Interaction Panel
        }
    }
}
