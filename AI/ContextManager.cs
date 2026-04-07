using System.Collections.Generic;

namespace DifferentWay.AI;

public class ContextManager
{
    public string SystemRules_P0 { get; set; } = "You are a game AI."; // ~300 tokens
    public string CurrentObjective_P1 { get; set; } = string.Empty;    // ~200 tokens
    public string WorldState_P2 { get; private set; } = string.Empty;  // ~500 tokens
    public List<string> RagContext_P3 { get; set; } = new();           // ~600 tokens
    public Queue<string> RecentChat_P4 { get; set; } = new();          // Remaining tokens (FIFO)

    public void UpdateWorldState(DifferentWay.Core.TimeManager timeManager, DifferentWay.Systems.StatManager playerStats)
    {
        WorldState_P2 = $"Time: Day {timeManager.CurrentDay}, {timeManager.CurrentHour:D2}:{timeManager.CurrentMinute:D2}. " +
                        $"Player HP: {playerStats.CurrentHP}/{playerStats.MaxHP}. " +
                        $"Player Karma: {playerStats.Karma}. " +
                        $"Player Charisma: {playerStats.Charisma}.";
    }

    public void InjectNpcContext(DifferentWay.Systems.NpcState npc)
    {
        CurrentObjective_P1 = $"You are {npc.Name}, a {npc.Profession}. Your current internal goal is: {npc.CurrentGoal}. You are currently at: {npc.CurrentLocationId}.";
    }

    public void FetchRagMemories(string prompt, DifferentWay.Database.MemoryManager memoryManager)
    {
        RagContext_P3.Clear();
        if (memoryManager != null)
        {
            var memories = memoryManager.FetchRelevantMemories(prompt);
            foreach (var mem in memories)
            {
                RagContext_P3.Add(mem);
            }
        }
    }

    public void AddChatHistory(string message)
    {
        RecentChat_P4.Enqueue(message);
        // Truncate logic would go here based on tokenizer counts
        if (RecentChat_P4.Count > 10)
        {
            RecentChat_P4.Dequeue();
        }
    }
}
