using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DifferentWay.Systems.Models;

public class AINpcState
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("profession")] public string Profession { get; set; } = string.Empty;
    [JsonPropertyName("goap_goal")] public string GoapGoal { get; set; } = string.Empty;
}

public class AIContext
{
    [JsonPropertyName("location")] public string Location { get; set; } = string.Empty;
    [JsonPropertyName("npc_state")] public AINpcState NpcState { get; set; } = new();
    [JsonPropertyName("player_action")] public string PlayerAction { get; set; } = string.Empty;
}

public class AIRequestPayload
{
    [JsonPropertyName("context")] public AIContext Context { get; set; } = new();
}

public class AIActionTrigger
{
    [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("amount")] public int Amount { get; set; }
}

public class AIResponse
{
    [JsonPropertyName("thoughts")] public string Thoughts { get; set; } = string.Empty;
    [JsonPropertyName("spoken_text")] public string SpokenText { get; set; } = string.Empty;
    [JsonPropertyName("action_triggers")] public List<AIActionTrigger>? ActionTriggers { get; set; }
}
