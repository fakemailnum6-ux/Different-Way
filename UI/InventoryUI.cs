using Godot;
using System.Collections.Generic;
using DifferentWay.Systems;
using DifferentWay.Core;

namespace DifferentWay.UI
{
    public partial class InventoryUI : Control
    {
        private VBoxContainer _itemList;
        private DataManager _dataManager;

        public override void _Ready()
        {
            SetAnchorsPreset(LayoutPreset.FullRect);
            Visible = false; // Hidden by default

            _dataManager = GetNodeOrNull<DataManager>("/root/DataManager");

            var panel = new Panel();
            panel.SetAnchorsPreset(LayoutPreset.FullRect);
            AddChild(panel);

            var margin = new MarginContainer();
            margin.SetAnchorsPreset(LayoutPreset.FullRect);
            margin.AddThemeConstantOverride("margin_all", 20);
            panel.AddChild(margin);

            var vbox = new VBoxContainer();
            margin.AddChild(vbox);

            var title = new Label { Text = "Инвентарь", HorizontalAlignment = HorizontalAlignment.Center };
            vbox.AddChild(title);

            var scroll = new ScrollContainer { SizeFlagsVertical = SizeFlags.ExpandFill };
            vbox.AddChild(scroll);

            _itemList = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            scroll.AddChild(_itemList);

            var closeBtn = new Button { Text = "Закрыть" };
            closeBtn.Pressed += () => Visible = false;
            vbox.AddChild(closeBtn);
        }

        public void UpdateInventory(Dictionary<string, int> playerInventory)
        {
            if (_dataManager == null) return;

            // Clear old list
            foreach (Node child in _itemList.GetChildren())
            {
                child.QueueFree();
            }

            foreach (var kvp in playerInventory)
            {
                string itemId = kvp.Key;
                int quantity = kvp.Value;

                string itemName = "Неизвестно";

                // Lookup name from DataManager
                if (_dataManager.Weapons.TryGetValue(itemId, out var w)) itemName = w.Name;
                else if (_dataManager.Armors.TryGetValue(itemId, out var a)) itemName = a.Name;
                else if (_dataManager.Consumables.TryGetValue(itemId, out var c)) itemName = c.Name;
                else if (_dataManager.Materials.TryGetValue(itemId, out var m)) itemName = m.Name;

                var itemRow = new HBoxContainer();

                var label = new Label { Text = $"{itemName} x{quantity}", SizeFlagsHorizontal = SizeFlags.ExpandFill };
                itemRow.AddChild(label);

                var useBtn = new Button { Text = "Использовать/Надеть" };
                useBtn.Pressed += () => OnItemUsed(itemId);
                itemRow.AddChild(useBtn);

                _itemList.AddChild(itemRow);
            }
        }

        private void OnItemUsed(string itemId)
        {
            var eventBus = GetNodeOrNull<EventBus>("/root/EventBus");

            // Simple stub equip/consume logic
            if (_dataManager != null)
            {
                if (_dataManager.Consumables.ContainsKey(itemId))
                {
                     eventBus?.EmitLogMessage("INFO", $"Выпито/Съедено: {itemId}. Статы восстановлены (симуляция).");
                     // We would call StatManager/Simulation to increase HP/Stamina here
                }
                else if (_dataManager.Weapons.ContainsKey(itemId) || _dataManager.Armors.ContainsKey(itemId))
                {
                     eventBus?.EmitLogMessage("INFO", $"Экипировано: {itemId}.");
                     // We would update CharacterStats.Equipment and call StatManager.RecalculateStats() here
                }
                else
                {
                     eventBus?.EmitLogMessage("INFO", $"Использован предмет: {itemId}");
                }
            }
        }
    }
}
