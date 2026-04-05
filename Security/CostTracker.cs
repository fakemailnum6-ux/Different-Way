using System;
using Godot;

namespace DifferentWay.Security
{
    public class CostTracker
    {
        public int TotalTokens { get; private set; } = 0;
        public int DailyLimit { get; set; } = 50000;

        public void TrackUsage(string prompt, string response)
        {
            // Simple word count as a mock tokenizer
            int inputTokens = prompt.Split(new char[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
            int outputTokens = response.Split(new char[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;

            TotalTokens += inputTokens + outputTokens;

            GD.Print($"Cost Tracker: Used {inputTokens + outputTokens} tokens. Total Session: {TotalTokens} / Limit: {DailyLimit}. Estimated Cost: ${(TotalTokens / 1000.0) * 0.01:F4}");

            if (TotalTokens > DailyLimit)
            {
                GD.PrintErr("WARNING: Daily token limit exceeded!");
            }
        }
    }
}
