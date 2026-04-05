using System.Collections.Generic;

namespace DifferentWay.AI;

public class ContextManager
{
    public string SystemRules_P0 { get; set; } = "You are a game AI."; // ~300 tokens
    public string CurrentObjective_P1 { get; set; } = string.Empty;    // ~200 tokens
    public string WorldState_P2 { get; set; } = string.Empty;          // ~500 tokens
    public List<string> RagContext_P3 { get; set; } = new();           // ~600 tokens
    public Queue<string> RecentChat_P4 { get; set; } = new();          // Remaining tokens (FIFO)

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
