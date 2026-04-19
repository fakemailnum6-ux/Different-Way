using Godot;
using System.Collections.Generic;
using DifferentWay.Systems;

namespace DifferentWay.UI
{
    public partial class QuestJournalUI : Control
    {
        private VBoxContainer _questList;
        private QuestManager _questManager;

        public override void _Ready()
        {
            SetAnchorsPreset(LayoutPreset.FullRect);
            Visible = false; // Hidden by default

            _questManager = GetNodeOrNull<QuestManager>("/root/QuestManager");

            var panel = new Panel();
            panel.SetAnchorsPreset(LayoutPreset.FullRect);
            AddChild(panel);

            var margin = new MarginContainer();
            margin.SetAnchorsPreset(LayoutPreset.FullRect);
            margin.AddThemeConstantOverride("margin_all", 20);
            panel.AddChild(margin);

            var vbox = new VBoxContainer();
            margin.AddChild(vbox);

            var title = new Label { Text = "Журнал заданий", HorizontalAlignment = HorizontalAlignment.Center };
            vbox.AddChild(title);

            var scroll = new ScrollContainer { SizeFlagsVertical = SizeFlags.ExpandFill };
            vbox.AddChild(scroll);

            _questList = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            scroll.AddChild(_questList);

            var closeBtn = new Button { Text = "Закрыть" };
            closeBtn.Pressed += () => Visible = false;
            vbox.AddChild(closeBtn);
        }

        public void OpenJournal()
        {
            if (_questManager == null) return;

            // Clear old list
            foreach (Node child in _questList.GetChildren())
            {
                child.QueueFree();
            }

            var quests = _questManager.GetQuests();

            foreach (var q in quests)
            {
                var qBox = new VBoxContainer();

                var titleLbl = new Label { Text = $"[{q.Status}] {q.Title}" };
                qBox.AddChild(titleLbl);

                var descLbl = new Label { Text = q.Description, AutowrapMode = TextServer.AutowrapMode.WordSmart };
                qBox.AddChild(descLbl);

                foreach (var obj in q.Objectives)
                {
                    var objLbl = new Label { Text = $" - {obj.Type} ({obj.TargetId}): {obj.CurrentAmount}/{obj.RequiredAmount}" };
                    qBox.AddChild(objLbl);
                }

                _questList.AddChild(qBox);
            }

            Visible = true;
        }
    }
}
