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

                // Validate item granting
                if (action.Type == "give_item")
                {
                    if (action.Amount > 10) return false; // Hard limit on item stacks

                    // Validate item actually exists in DataManager
                    bool itemExists = DifferentWay.Systems.DataManager.Weapons.ContainsKey(action.Id) ||
                                      DifferentWay.Systems.DataManager.Armors.ContainsKey(action.Id) ||
                                      DifferentWay.Systems.DataManager.Consumables.ContainsKey(action.Id) ||
                                      DifferentWay.Systems.DataManager.Materials.ContainsKey(action.Id);

                    if (!itemExists) return false; // AI hallucinated an item
                }

                // Validate quest granting
                if (action.Type == "give_quest")
                {
                    if (!DifferentWay.Systems.DataManager.StarterQuests.ContainsKey(action.Id))
                    {
                        return false; // AI hallucinated a non-existent quest
                    }
                }

                // Validate quest turn-in
                if (action.Type == "complete_quest")
                {
                    if (!DifferentWay.Systems.DataManager.StarterQuests.ContainsKey(action.Id))
                    {
                        return false; // AI hallucinated a non-existent quest
                    }
                }
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
