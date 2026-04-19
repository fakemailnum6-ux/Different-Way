using Godot;
using System.Collections.Generic;
using DifferentWay.Systems;

namespace DifferentWay.UI
{
    public partial class CraftingUI : Control
    {
        private VBoxContainer _recipeList;
        private CraftingEngine _craftingEngine;

        public override void _Ready()
        {
            SetAnchorsPreset(LayoutPreset.FullRect);
            Visible = false; // Hidden by default

            _craftingEngine = GetNodeOrNull<CraftingEngine>("/root/CraftingEngine");

            var panel = new Panel();
            panel.SetAnchorsPreset(LayoutPreset.FullRect);
            AddChild(panel);

            var margin = new MarginContainer();
            margin.SetAnchorsPreset(LayoutPreset.FullRect);
            margin.AddThemeConstantOverride("margin_all", 20);
            panel.AddChild(margin);

            var vbox = new VBoxContainer();
            margin.AddChild(vbox);

            var title = new Label { Text = "Верстак (Крафт)", HorizontalAlignment = HorizontalAlignment.Center };
            vbox.AddChild(title);

            var scroll = new ScrollContainer { SizeFlagsVertical = SizeFlags.ExpandFill };
            vbox.AddChild(scroll);

            _recipeList = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            scroll.AddChild(_recipeList);

            var closeBtn = new Button { Text = "Закрыть" };
            closeBtn.Pressed += () => Visible = false;
            vbox.AddChild(closeBtn);
        }

        public void OpenCrafting(CharacterStats crafter, Dictionary<string, int> inventory)
        {
            if (_craftingEngine == null) return;

            // Clear old list
            foreach (Node child in _recipeList.GetChildren())
            {
                child.QueueFree();
            }

            var recipes = _craftingEngine.GetRecipes();

            foreach (var r in recipes)
            {
                var row = new HBoxContainer();

                string reqText = $"{r.ResultItemId} (Требует INT: {r.RequiredInt}) - ";
                foreach (var req in r.RequiredMaterials)
                {
                    reqText += $"{req.Key}x{req.Value} ";
                }

                var label = new Label { Text = reqText, SizeFlagsHorizontal = SizeFlags.ExpandFill };
                row.AddChild(label);

                var craftBtn = new Button { Text = "Скрафтить" };

                // Color formatting logic for missing materials
                bool canCraft = _craftingEngine.CanCraft(inventory, r);
                if (!canCraft)
                {
                    craftBtn.Disabled = true;
                    label.Modulate = new Color(1, 0.5f, 0.5f); // Red tint for missing items
                }

                craftBtn.Pressed += () => {
                    _craftingEngine.TryCraft(crafter, inventory, r);
                    OpenCrafting(crafter, inventory); // Refresh list
                };

                row.AddChild(craftBtn);
                _recipeList.AddChild(row);
            }

            Visible = true;
        }
    }
}
