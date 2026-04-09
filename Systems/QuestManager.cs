using Godot;
using System.Collections.Generic;
using DifferentWay.Core;

namespace DifferentWay.Systems
{
    public enum QuestStatus
    {
        Active,
        Completed,
        Failed
    }

    public class QuestObjective
    {
        public string Type { get; set; } // "kill_mob", "fetch_item"
        public string TargetId { get; set; } // e.g. "wolf_01"
        public int CurrentAmount { get; set; } = 0;
        public int RequiredAmount { get; set; }
    }

    public class QuestNode
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public QuestStatus Status { get; set; } = QuestStatus.Active;
        public List<QuestObjective> Objectives { get; set; } = new List<QuestObjective>();
        public int GoldReward { get; set; }
    }

    public partial class QuestManager : Node
    {
        private EventBus _eventBus;
        private List<QuestNode> _activeQuests = new List<QuestNode>();

        public override void _Ready()
        {
            _eventBus = GetNodeOrNull<EventBus>("/root/EventBus");
        }

        public void AddQuest(QuestNode quest)
        {
            _activeQuests.Add(quest);
            _eventBus?.EmitLogMessage("INFO", $"Новое задание: {quest.Title}");
        }

        public void ProcessKill(string mobId)
        {
            foreach (var q in _activeQuests)
            {
                if (q.Status != QuestStatus.Active) continue;

                foreach (var obj in q.Objectives)
                {
                    if (obj.Type == "kill_mob" && obj.TargetId == mobId)
                    {
                        if (obj.CurrentAmount < obj.RequiredAmount)
                        {
                            obj.CurrentAmount++;
                            _eventBus?.EmitLogMessage("INFO", $"Прогресс: {obj.CurrentAmount}/{obj.RequiredAmount} убито {mobId}");

                            CheckQuestCompletion(q);
                        }
                    }
                }
            }
        }

        private void CheckQuestCompletion(QuestNode quest)
        {
            foreach (var obj in quest.Objectives)
            {
                if (obj.CurrentAmount < obj.RequiredAmount) return; // Still running
            }

            quest.Status = QuestStatus.Completed;
            _eventBus?.EmitLogMessage("INFO", $"Задание выполнено: {quest.Title}");

            // Give reward logic
            // E.g. playerGold += quest.GoldReward;
        }

        public List<QuestNode> GetQuests() => _activeQuests;
    }
}
