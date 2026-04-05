using System;
using System.Collections.Generic;
using Godot;

namespace DifferentWay.AI
{
    public class ContextManager
    {
        private const int MaxTokens = 4000;
        private const int P0_Max = 300;
        private const int P1_Max = 200;
        private const int P2_Max = 500;
        private const int P3_Max = 500;
        private const int P4_Max = 2500;

        public string SystemRules { get; set; } = "You are in a dark fantasy setting. Never break the 4th wall.";
        public string CurrentObjective { get; set; } = "Player failed charisma check, refuse them.";
        public string WorldState { get; set; } = "Location: Tavern, Time: Night.";
        public string HistorySummary { get; set; } = "Player previously asked for a discount.";
        public List<string> ChatHistory { get; set; } = new List<string>();

        public void AddChatMessage(string message)
        {
            ChatHistory.Add(message);
            // In a real implementation, we would evict older messages to HistorySummary if token limit exceeded
            if (ChatHistory.Count > 10)
            {
                ChatHistory.RemoveAt(0); // Simple FIFO eviction
            }
        }
    }
}
