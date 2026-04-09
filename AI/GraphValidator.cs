using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using DifferentWay.Systems;

namespace DifferentWay.AI
{
    // DTOs for the AI JSON response
    public class AIActionTrigger
    {
        [JsonPropertyName("type")] public string Type { get; set; }
        [JsonPropertyName("id")] public string Id { get; set; }
        [JsonPropertyName("amount")] public int? Amount { get; set; }
    }

    public class AIResponse
    {
        [JsonPropertyName("thoughts")] public string Thoughts { get; set; }
        [JsonPropertyName("spoken_text")] public string SpokenText { get; set; }
        [JsonPropertyName("action_triggers")] public List<AIActionTrigger> ActionTriggers { get; set; }
    }

    public partial class GraphValidator : Node
    {
        private DataManager _dataManager;
        private Core.EventBus _eventBus;

        public override void _Ready()
        {
            _dataManager = GetNodeOrNull<DataManager>("/root/DataManager");
            _eventBus = GetNodeOrNull<Core.EventBus>("/root/EventBus");
        }

        public AIResponse ValidateAndParse(string rawJson)
        {
            // Level 1: Syntax Check
            string cleanJson = CleanJsonString(rawJson);
            if (string.IsNullOrEmpty(cleanJson))
            {
                return null; // Fallback will be triggered by caller
            }

            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var response = JsonSerializer.Deserialize<AIResponse>(cleanJson, options);

                if (string.IsNullOrEmpty(response?.SpokenText))
                {
                    _eventBus?.EmitLogMessage("WARNING", "AI returned JSON without spoken_text");
                    return null;
                }

                // Level 2: Semantic check against Skeleton (Data Registry Match)
                if (response.ActionTriggers != null && _dataManager != null)
                {
                    var validTriggers = new List<AIActionTrigger>();
                    foreach (var trigger in response.ActionTriggers)
                    {
                        if (trigger.Type == "give_item")
                        {
                            // Validate item exists
                            if (IsItemValid(trigger.Id))
                            {
                                validTriggers.Add(trigger);
                            }
                            else
                            {
                                _eventBus?.EmitLogMessage("WARNING", $"AI hallucinated item ID: {trigger.Id}. Trigger ignored.");
                            }
                        }
                        else
                        {
                            // Other triggers (end_dialogue, attack) are passed through
                            validTriggers.Add(trigger);
                        }
                    }
                    response.ActionTriggers = validTriggers;
                }

                return response;
            }
            catch (Exception ex)
            {
                _eventBus?.EmitLogMessage("ERROR", $"Failed to parse AI JSON: {ex.Message}\nRaw: {cleanJson}");
                return null;
            }
        }

        private bool IsItemValid(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return false;

            return _dataManager.Weapons.ContainsKey(itemId) ||
                   _dataManager.Armors.ContainsKey(itemId) ||
                   _dataManager.Consumables.ContainsKey(itemId) ||
                   _dataManager.Materials.ContainsKey(itemId);
        }

        private string CleanJsonString(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return string.Empty;

            int startIndex = raw.IndexOf('{');
            int endIndex = raw.LastIndexOf('}');

            if (startIndex >= 0 && endIndex > startIndex)
            {
                return raw.Substring(startIndex, endIndex - startIndex + 1);
            }
            return string.Empty;
        }
    }
}
