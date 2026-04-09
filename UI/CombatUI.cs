using Godot;
using DifferentWay.Systems;

namespace DifferentWay.UI
{
    public partial class CombatUI : Control
    {
        private RichTextLabel _combatLog;
        private Label _playerStats;
        private Label _enemyStats;
        private Button _attackButton;
        private Button _fleeButton;

        private CombatManager _combatManager;

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

            _attackButton.Pressed += () => _combatManager?.PlayerAttack();
            _fleeButton.Pressed += () => _combatManager?.PlayerFlee();

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

            _combatManager = GetNodeOrNull<CombatManager>("/root/CombatManager");
            if (_combatManager != null)
            {
                _combatManager.OnCombatStateChanged += OnStateChanged;
                _combatManager.OnCombatUpdate += OnUpdate;
            }
        }

        public void StartCombat(string enemyName, int enemyHp)
        {
            _combatLog.Text = $"[center]Враг: {enemyName}![/center]\n";
            _enemyStats.Text = $"{enemyName}\nHP: {enemyHp}/{enemyHp}";
            Visible = true;

            // In full game this triggers CombatManager setup. For phase 1/2 integration:
            var pStats = new CharacterStats { CurrentHP = 100, MaxHP = 100 };
            var eStats = new CharacterStats { CurrentHP = enemyHp, MaxHP = enemyHp };
            _combatManager?.StartCombat(pStats, eStats, null, null);
        }

        private void OnStateChanged(CombatState state)
        {
            if (state == CombatState.PlayerTurn)
            {
                _attackButton.Disabled = false;
                _fleeButton.Disabled = false;
            }
            else if (state == CombatState.End)
            {
                 _attackButton.Disabled = true;
                 _fleeButton.Disabled = true;
                 Visible = false; // End combat
            }
            else
            {
                _attackButton.Disabled = true;
                _fleeButton.Disabled = true;
            }
        }

        private void OnUpdate(string pName, int php, int pmaxhp, string eName, int ehp, int emaxhp)
        {
             _playerStats.Text = $"{pName}\nHP: {php}/{pmaxhp}";
             _enemyStats.Text = $"{eName}\nHP: {ehp}/{emaxhp}";
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

        public override void _ExitTree()
        {
             if (_combatManager != null)
             {
                  _combatManager.OnCombatStateChanged -= OnStateChanged;
                  _combatManager.OnCombatUpdate -= OnUpdate;
             }
        }
    }
}
