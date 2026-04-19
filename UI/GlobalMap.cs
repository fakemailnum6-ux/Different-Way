using Godot;
using System.Collections.Generic;
using DifferentWay.Systems;
using DifferentWay.Core;

namespace DifferentWay.UI
{
    public partial class GlobalMap : Control
    {
        private KingdomNode _currentKingdom;
        private EventBus _eventBus;

        // Caching positions for line drawing
        private Dictionary<string, Vector2> _settlementPositions = new Dictionary<string, Vector2>();

        public override void _Ready()
        {
            SetAnchorsPreset(LayoutPreset.FullRect);

            _eventBus = GetNodeOrNull<EventBus>("/root/EventBus");

            // Background
            var bg = new ColorRect { Color = new Color(0.05f, 0.15f, 0.05f) }; // Dark green for overworld
            bg.SetAnchorsPreset(LayoutPreset.FullRect);
            AddChild(bg);

            var title = new Label
            {
                Text = "ГЛОБАЛЬНАЯ КАРТА",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Position = new Vector2(0, 20)
            };
            title.SetAnchorsPreset(LayoutPreset.TopWide);
            AddChild(title);
        }

        public void LoadKingdom(KingdomNode kingdom)
        {
            _currentKingdom = kingdom;
            _settlementPositions.Clear();

            // Clear old children (except background and title)
            foreach (Node child in GetChildren())
            {
                if (child is ColorRect c && c.Color == new Color(0.05f, 0.15f, 0.05f)) continue;
                if (child is Label l && l.Text == "ГЛОБАЛЬНАЯ КАРТА") continue;
                child.QueueFree();
            }

            if (_currentKingdom == null) return;

            // Generate some deterministic positions for settlements (stub layout)
            float spacing = 200f;
            Vector2 startPos = new Vector2(200, 200);

            for (int i = 0; i < _currentKingdom.Settlements.Count; i++)
            {
                var s = _currentKingdom.Settlements[i];
                // Simple staggered grid
                Vector2 pos = startPos + new Vector2(i * spacing, (i % 2 == 0) ? 0 : 150);
                _settlementPositions[s.Id] = pos;
            }

            // Draw Paths between all settlements (fully connected graph for simplicity)
            if (_currentKingdom.Settlements.Count > 1)
            {
                for (int i = 0; i < _currentKingdom.Settlements.Count - 1; i++)
                {
                    string fromId = _currentKingdom.Settlements[i].Id;
                    string toId = _currentKingdom.Settlements[i + 1].Id;

                    var line = new Line2D
                    {
                        DefaultColor = new Color(0.8f, 0.7f, 0.2f),
                        Width = 6f
                    };
                    line.AddPoint(_settlementPositions[fromId]);
                    line.AddPoint(_settlementPositions[toId]);
                    AddChild(line);
                }
            }

            // Draw Settlements (Buttons)
            foreach (var s in _currentKingdom.Settlements)
            {
                var pos = _settlementPositions[s.Id];

                var btn = new Button
                {
                    Text = s.Name,
                    Position = pos - new Vector2(50, 25), // Center
                    CustomMinimumSize = new Vector2(100, 50)
                };

                btn.Pressed += () => OnSettlementClicked(s.Id);

                AddChild(btn);
            }
        }

        private void OnSettlementClicked(string settlementId)
        {
            _eventBus?.EmitLogMessage("INFO", $"Путешествуем в: {settlementId}");

            // In a full implementation, we'd wait for simulation time to pass.
            // For now, we instantly request a state change or UI swap.
            // Here we assume Main.cs or GameManager handles the actual node swap.
            // We just hide ourselves.
            Visible = false;
        }
    }
}
