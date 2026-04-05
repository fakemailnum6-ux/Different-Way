using System.Collections.Generic;

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

public class QuestManager
{
    private List<QuestGraph> _activeQuests = new();

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

    public void TrackProgress(string actionType, string targetId, int amount)
    {
        foreach (var quest in _activeQuests)
        {
            foreach (var step in quest.Steps)
            {
                if (!step.IsComplete && step.TargetId == targetId)
                {
                    step.CurrentAmount += amount;
                    // Trigger events if step or quest completes here
                }
            }
        }
    }
}
