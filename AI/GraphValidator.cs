using Godot;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class AiActionTriggerDto
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("amount")]
    public int? Amount { get; set; }
}

public class AiResponseDto
{
    [JsonProperty("thoughts")]
    public string Thoughts { get; set; }

    [JsonProperty("spoken_text")]
    public string SpokenText { get; set; }

    [JsonProperty("action_triggers")]
    public List<AiActionTriggerDto> ActionTriggers { get; set; } = new List<AiActionTriggerDto>();
}

public partial class GraphValidator : RefCounted
{
    /// <summary>
    /// Parses the raw AI string into a verified DTO.
    /// Level 1: Soft Recovery (removes markdown fences).
    /// </summary>
    public AiResponseDto ParseAndValidateSyntax(string rawJson)
    {
        string cleanedJson = SanitizeJson(rawJson);

        try
        {
            var dto = JsonConvert.DeserializeObject<AiResponseDto>(cleanedJson);
            if (dto == null || string.IsNullOrWhiteSpace(dto.SpokenText))
            {
                throw new Exception("JSON parsed but missing required spoken_text.");
            }
            return dto;
        }
        catch (Exception ex)
        {
            ServiceLocator.Logger.LogError($"GraphValidator: Syntax Parse Failed. {ex.Message}");
            return GenerateFallbackDto();
        }
    }

    /// <summary>
    /// Level 2: Semantic Verification against DataManager Skeleton.
    /// Filters out hallucinated items/mobs requested by the AI.
    /// </summary>
    public void ProcessActionTriggers(AiResponseDto response)
    {
        if (response.ActionTriggers == null || response.ActionTriggers.Count == 0) return;

        var validTriggers = new List<AiActionTriggerDto>();

        foreach (var trigger in response.ActionTriggers)
        {
            if (trigger.Type == "give_item")
            {
                if (IsValidItem(trigger.Id))
                {
                    validTriggers.Add(trigger);
                }
                else
                {
                    ServiceLocator.Logger.LogWarning($"GraphValidator: Hallucinated item '{trigger.Id}' ignored.");
                }
            }
            else if (trigger.Type == "end_dialogue")
            {
                validTriggers.Add(trigger);
            }
            else
            {
                // Accept unknown types to let game systems handle them
                validTriggers.Add(trigger);
            }
        }

        response.ActionTriggers = validTriggers;
    }

    private bool IsValidItem(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return false;
        var data = ServiceLocator.DataManager;

        return data.Weapons.ContainsKey(itemId) ||
               data.Armors.ContainsKey(itemId) ||
               data.Consumables.ContainsKey(itemId) ||
               data.Materials.ContainsKey(itemId);
    }

    private string SanitizeJson(string raw)
    {
        string text = raw.Trim();

        // Soft Recovery: Remove ```json fences
        if (text.StartsWith("```json"))
        {
            text = text.Substring(7);
        }
        if (text.StartsWith("```"))
        {
            text = text.Substring(3);
        }
        if (text.EndsWith("```"))
        {
            text = text.Substring(0, text.Length - 3);
        }

        // Find first and last brace in case of prepended/appended chatter
        int firstBrace = text.IndexOf('{');
        int lastBrace = text.LastIndexOf('}');

        if (firstBrace >= 0 && lastBrace >= 0 && lastBrace > firstBrace)
        {
            return text.Substring(firstBrace, lastBrace - firstBrace + 1);
        }

        return text;
    }

    private AiResponseDto GenerateFallbackDto()
    {
        string fallbackText = "The NPC remains silent.";
        if (ServiceLocator.LocalizationManager != null)
        {
            fallbackText = ServiceLocator.LocalizationManager.Translate("fallback_ai_error");
        }

        return new AiResponseDto
        {
            Thoughts = "Fallback triggered due to parse error.",
            SpokenText = fallbackText,
            ActionTriggers = new List<AiActionTriggerDto> { new AiActionTriggerDto { Type = "end_dialogue" } }
        };
    }
}
