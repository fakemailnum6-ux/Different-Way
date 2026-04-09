using Godot;

namespace DifferentWay.UI
{
    public partial class CombatUI : Control
    {
        private RichTextLabel _combatLog;
        private Label _playerStats;
        private Label _enemyStats;
        private Button _attackButton;
        private Button _fleeButton;

        public override void _Ready()
        {
            SetAnchorsPreset(LayoutPreset.FullRect);

            // Split into HBox
            var hBox = new HBoxContainer();
            hBox.SetAnchorsPreset(LayoutPreset.FullRect);
            AddChild(hBox);

            // Left side (Player)
            var leftPanel = new PanelContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            _playerStats = new Label { Text = "Игрок\nHP: 100/100" };
            var vLeft = new VBoxContainer();
            vLeft.AddChild(_playerStats);
            leftPanel.AddChild(vLeft);
            hBox.AddChild(leftPanel);

            // Center (Log + Actions)
            var centerPanel = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsStretchRatio = 2 };
            _combatLog = new RichTextLabel { SizeFlagsVertical = SizeFlags.ExpandFill, BbcodeEnabled = true, ScrollFollowing = true };
            _combatLog.Text = "[center]Бой начался![/center]";

            var btnBox = new HBoxContainer();
            _attackButton = new Button { Text = "Атаковать", SizeFlagsHorizontal = SizeFlags.ExpandFill };
            _fleeButton = new Button { Text = "Сбежать", SizeFlagsHorizontal = SizeFlags.ExpandFill };

            btnBox.AddChild(_attackButton);
            btnBox.AddChild(_fleeButton);

            centerPanel.AddChild(_combatLog);
            centerPanel.AddChild(btnBox);
            hBox.AddChild(centerPanel);

            // Right side (Enemy)
            var rightPanel = new PanelContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            _enemyStats = new Label { Text = "Противник\nHP: ??" };
            var vRight = new VBoxContainer();
            vRight.AddChild(_enemyStats);
            rightPanel.AddChild(vRight);
            hBox.AddChild(rightPanel);

            Visible = false; // Hidden by default
        }

        public void StartCombat(string enemyName, int enemyHp)
        {
            _combatLog.Text = $"[center]Враг: {enemyName}![/center]\n";
            _enemyStats.Text = $"{enemyName}\nHP: {enemyHp}/{enemyHp}";
            Visible = true;
        }

        public void AppendLog(string message)
        {
            _combatLog.AppendText(message + "\n");
        }

        public void EndCombat()
        {
            Visible = false;
            _combatLog.Text = "";
        }
    }
}
