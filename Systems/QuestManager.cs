using System.Collections.Generic;
using Godot;

namespace DifferentWay.Systems;

public class QuestNode
{
    public string Description { get; set; } = string.Empty;
    public string TargetId { get; set; } = string.Empty;
    public int RequiredAmount { get; set; }
    public int CurrentAmount { get; set; }

    public bool IsComplete => CurrentAmount >= RequiredAmount;
}

public class QuestGraph
{
    public string Id { get; set; } = string.Empty;
    public string AiTitle { get; set; } = string.Empty;
    public string AiLoreReason { get; set; } = string.Empty;

    public List<QuestNode> Steps { get; set; } = new();

    public bool IsComplete()
    {
        foreach (var step in Steps)
        {
            if (!step.IsComplete) return false;
        }
        return true;
    }
}

public partial class QuestManager : RefCounted
{
    private List<QuestGraph> _activeQuests = new();

    public void GenerateAndAcceptTestQuest(string title)
    {
        var quest = new QuestGraph
        {
            Id = "quest_" + System.Guid.NewGuid().ToString().Substring(0, 8),
            AiTitle = title,
            AiLoreReason = "Сгенерировано для теста"
        };
        quest.Steps.Add(new QuestNode { Description = "Тестовая цель", TargetId = "dummy_target", RequiredAmount = 1 });
        AcceptAiGeneratedQuest(quest);
    }

    public string[] GetActiveQuestTitles()
    {
        var titles = new List<string>();
        foreach (var q in _activeQuests)
        {
            string progressStr = "";
            foreach (var step in q.Steps)
            {
                progressStr += $"({step.CurrentAmount}/{step.RequiredAmount}) ";
            }
            string state = q.IsComplete() ? "[Выполнено] " : "";
            titles.Add(state + q.AiTitle + " " + progressStr.Trim());
        }
        return titles.ToArray();
    }

    public QuestGraph BuildMathematicalQuestSkeleton()
    {
        // 4.5 Сборка Скелета: [Node A: Идти в Пещеру] -> [Node B: Убить Бандита] -> [Node C: Получить Золото]
        var quest = new QuestGraph();

        quest.Steps.Add(new QuestNode { Description = "Идти в Пещеру", TargetId = "loc_cave_01", RequiredAmount = 1 });
        quest.Steps.Add(new QuestNode { Description = "Убить Бандита", TargetId = "mob_bandit", RequiredAmount = 3 });
        quest.Steps.Add(new QuestNode { Description = "Получить Золото", TargetId = "item_gold", RequiredAmount = 50 });

        return quest;
    }

    public void AcceptAiGeneratedQuest(QuestGraph questWithMeat)
    {
        // 4.5 Исполнение: AI disconnects, locally track counters
        _activeQuests.Add(questWithMeat);
    }

    public bool AcceptQuestById(string questId)
    {
        if (DifferentWay.Systems.DataManager.StarterQuests.TryGetValue(questId, out var template))
        {
            // Simple deep clone to avoid modifying the DataManager template
            var cloneStr = System.Text.Json.JsonSerializer.Serialize(template);
            var clone = System.Text.Json.JsonSerializer.Deserialize<QuestGraph>(cloneStr);

            if (clone != null)
            {
                _activeQuests.Add(clone);
                DifferentWay.Core.GameLogger.Log($"Получен новый квест: {clone.AiTitle}");
                return true;
            }
        }
        return false;
    }

    public bool TryCompleteQuest(string questId, InventoryManager inventory)
    {
        for (int i = 0; i < _activeQuests.Count; i++)
        {
            if (_activeQuests[i].Id == questId)
            {
                if (_activeQuests[i].IsComplete())
                {
                    DifferentWay.Core.GameLogger.Log($"Квест '{_activeQuests[i].AiTitle}' успешно сдан!");
                    inventory.AddGold(50); // Hardcoded reward for Phase 3 mockup
                    DifferentWay.Core.GameLogger.Log("Вы получили 50 золота в награду.");
                    _activeQuests.RemoveAt(i);
                    return true;
                }
                else
                {
                    DifferentWay.Core.GameLogger.Log($"Квест '{_activeQuests[i].AiTitle}' еще не выполнен.");
                    return false;
                }
            }
        }
        DifferentWay.Core.GameLogger.Log($"У вас нет активного квеста '{questId}'.");
        return false;
    }

    public void TrackProgress(string actionType, string targetId, int amount)
    {
        UpdateQuestProgress(targetId, amount);
    }

    public void UpdateQuestProgress(string targetId, int amount)
    {
        foreach (var quest in _activeQuests)
        {
            foreach (var step in quest.Steps)
            {
                if (!step.IsComplete && step.TargetId == targetId)
                {
                    step.CurrentAmount += amount;
                    DifferentWay.Core.GameLogger.Log($"Прогресс квеста '{quest.AiTitle}': {step.CurrentAmount}/{step.RequiredAmount}");

                    if (step.IsComplete)
                    {
                        DifferentWay.Core.GameLogger.Log($"Этап квеста '{step.Description}' завершен!");
                    }

                    if (quest.IsComplete())
                    {
                        DifferentWay.Core.GameLogger.Log($"Квест '{quest.AiTitle}' полностью завершен!");
                        // Optionally trigger global event here
                    }
                }
            }
        }
    }
}
