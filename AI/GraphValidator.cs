using System.Text.Json;
using DifferentWay.Systems.Models;

namespace DifferentWay.AI;

public class GraphValidator
{
    // Evaluates whether the AI's requested actions are mathematically/logically allowed
    public bool ValidateAiResponse(string jsonResponse)
    {
        try
        {
            var response = JsonSerializer.Deserialize<AIResponse>(jsonResponse);

            if (response?.ActionTriggers == null)
                return true; // No actions to validate

            foreach (var action in response.ActionTriggers)
            {
                // Absolute defense against prompt injection exploiting local node limits
                if (action.Type == "give_gold" && action.Amount > 100)
                {
                    // Rejected: AI attempting to give too much gold
                    return false;
                }

                // Add more skeleton validation constraints here...
            }

            return true;
        }
        catch
        {
            // Invalid JSON
            return false;
        }
    }
}
