using System;
using System.Text;
using Godot;

namespace DifferentWay.AI
{
    public class PromptBuilder
    {
        private readonly ContextManager _contextManager;

        public PromptBuilder(ContextManager contextManager)
        {
            _contextManager = contextManager;
        }

        public string BuildPrompt()
        {
            var sb = new StringBuilder();

            // P0
            sb.AppendLine($"[SYSTEM RULES]: {_contextManager.SystemRules}");

            // P1
            sb.AppendLine($"[CURRENT OBJECTIVE]: {_contextManager.CurrentObjective}");

            // P2
            sb.AppendLine($"[WORLD STATE]: {_contextManager.WorldState}");

            // P3
            sb.AppendLine($"[HISTORY SUMMARY]: {_contextManager.HistorySummary}");

            // P4
            sb.AppendLine("[RECENT CHAT HISTORY]:");
            foreach (var msg in _contextManager.ChatHistory)
            {
                sb.AppendLine($"- {msg}");
            }

            return sb.ToString();
        }
    }
}
