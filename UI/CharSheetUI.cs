using Godot;
using DifferentWay.Systems;

namespace DifferentWay.UI
{
    public partial class CharSheetUI : Control
    {
        private Label _strLabel;
        private Label _dexLabel;
        private Label _endLabel;
        private Label _intLabel;
        private Label _luckLabel;

        private Label _hpLabel;
        private Label _staminaLabel;
        private Label _manaLabel;
        private Label _evasionLabel;

        public override void _Ready()
        {
            SetAnchorsPreset(LayoutPreset.FullRect);
            Visible = false; // Hidden by default

            var panel = new Panel();
            panel.SetAnchorsPreset(LayoutPreset.FullRect);
            AddChild(panel);

            var margin = new MarginContainer();
            margin.SetAnchorsPreset(LayoutPreset.FullRect);
            margin.AddThemeConstantOverride("margin_all", 20);
            panel.AddChild(margin);

            var vbox = new VBoxContainer();
            margin.AddChild(vbox);

            var title = new Label { Text = "Характеристики Персонажа", HorizontalAlignment = HorizontalAlignment.Center };
            vbox.AddChild(title);

            var grid = new GridContainer { Columns = 2, SizeFlagsVertical = SizeFlags.ExpandFill };
            vbox.AddChild(grid);

            // Primary
            _strLabel = new Label { Text = "Сила (STR): 10" };
            _dexLabel = new Label { Text = "Ловкость (DEX): 10" };
            _endLabel = new Label { Text = "Выносливость (END): 10" };
            _intLabel = new Label { Text = "Интеллект (INT): 10" };
            _luckLabel = new Label { Text = "Удача (LUCK): 5" };

            // Secondary
            _hpLabel = new Label { Text = "Здоровье: 100/100" };
            _staminaLabel = new Label { Text = "Усталость: 100/100" };
            _manaLabel = new Label { Text = "Мана: 50/50" };
            _evasionLabel = new Label { Text = "Уворот: 15%" };

            grid.AddChild(new Label { Text = "ОСНОВНЫЕ" });
            grid.AddChild(new Label { Text = "ВТОРИЧНЫЕ" });

            grid.AddChild(_strLabel);
            grid.AddChild(_hpLabel);
            grid.AddChild(_dexLabel);
            grid.AddChild(_staminaLabel);
            grid.AddChild(_endLabel);
            grid.AddChild(_manaLabel);
            grid.AddChild(_intLabel);
            grid.AddChild(_evasionLabel);
            grid.AddChild(_luckLabel);
            grid.AddChild(new Control()); // Spacer

            var closeBtn = new Button { Text = "Закрыть" };
            closeBtn.Pressed += () => Visible = false;
            vbox.AddChild(closeBtn);
        }

        public void UpdateStats(CharacterStats stats)
        {
            _strLabel.Text = $"Сила (STR): {stats.STR}";
            _dexLabel.Text = $"Ловкость (DEX): {stats.DEX}";
            _endLabel.Text = $"Выносливость (END): {stats.END}";
            _intLabel.Text = $"Интеллект (INT): {stats.INT}";
            _luckLabel.Text = $"Удача (LUCK): {stats.Luck}";

            _hpLabel.Text = $"Здоровье: {stats.CurrentHP}/{stats.MaxHP}";
            _staminaLabel.Text = $"Усталость: {stats.CurrentStamina}/{stats.MaxStamina}";
            _manaLabel.Text = $"Мана: {stats.CurrentMana}/{stats.MaxMana}";
            _evasionLabel.Text = $"Уворот: {stats.Evasion}%";
        }
    }
}
