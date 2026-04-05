using System.Text;

namespace DifferentWay.AI;

public class PromptBuilder
{
    private readonly ContextManager _contextManager;

    public PromptBuilder(ContextManager contextManager)
    {
        _contextManager = contextManager;
    }

    public string BuildFinalPrompt()
    {
        // Zero-Allocation Policy: Concat strings using StringBuilder
        StringBuilder sb = new StringBuilder();

        // P0: System Rules (Hard strict boundaries)
        sb.AppendLine("[SYSTEM P0]");
        sb.AppendLine(_contextManager.SystemRules_P0);

        // P1: GOAP & Objectives
        sb.AppendLine("[OBJECTIVE P1]");
        sb.AppendLine(_contextManager.CurrentObjective_P1);

        // P2: World State
        sb.AppendLine("[WORLD STATE P2]");
        sb.AppendLine(_contextManager.WorldState_P2);

        // P3: RAG Memory Context
        sb.AppendLine("[MEMORY P3]");
        foreach (var memory in _contextManager.RagContext_P3)
        {
            sb.AppendLine(memory);
        }

        // P4: Chat FIFO
        sb.AppendLine("[CHAT LOG P4]");
        foreach (var msg in _contextManager.RecentChat_P4)
        {
            sb.AppendLine(msg);
        }

        return sb.ToString();
    }
}
