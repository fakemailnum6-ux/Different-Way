using Godot;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class LocationParams
{
    public string Biome { get; set; }
    public string Type { get; set; }
    public string Faction { get; set; }
    public int PlayerLevel { get; set; }
    public string NarrativeContext { get; set; }
}

public class GeneratedBuilding
{
    public string Name { get; set; }
    public string Type { get; set; }
    public float PosX { get; set; }
    public float PosY { get; set; }
}

public class GeneratedNpc
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Profession { get; set; }
    public string InitialGoapGoal { get; set; }
    public List<string> InventoryIds { get; set; } = new List<string>();
}

public class GeneratedLocationResult
{
    public string LocationName { get; set; }
    public string Lore { get; set; }
    public List<GeneratedBuilding> Buildings { get; set; } = new List<GeneratedBuilding>();
    public List<GeneratedNpc> Npcs { get; set; } = new List<GeneratedNpc>();
}

public partial class WorldBuilder : RefCounted
{
    private readonly LLMClient _llmClient;

    public WorldBuilder(LLMClient llmClient)
    {
        _llmClient = llmClient;
    }

    /// <summary>
    /// Executes the pipeline from Arc.md Section 4.7
    /// </summary>
    public async Task<GeneratedLocationResult> GenerateLocation(LocationParams parameters, CancellationToken token = default)
    {
        ServiceLocator.Logger.LogInfo($"WorldBuilder: Initiating generation for {parameters.Type} in {parameters.Biome}...");

        string systemPrompt = @"You are a world-building engine. Output strictly valid JSON matching this structure:
{
  ""location_name"": ""Name"",
  ""lore"": ""General lore based on context"",
  ""buildings"": [{ ""name"": ""Tavern"", ""type"": ""tavern"" }],
  ""npcs"": [{ ""id"": ""npc_01"", ""name"": ""John"", ""profession"": ""blacksmith"", ""initial_goap_goal"": ""Work"", ""inventory_ids"": [""sword_iron""] }]
}";

        string userPrompt = $"Biome: {parameters.Biome}\nType: {parameters.Type}\nFaction: {parameters.Faction}\nPlayerLevel: {parameters.PlayerLevel}\nContext: {parameters.NarrativeContext}";

        try
        {
            // 1. Meat Generation (LLM Call)
            string rawJson = await _llmClient.RequestPromptAsync(systemPrompt, userPrompt, token);

            // Temporary soft-recovery inline logic
            if (rawJson.StartsWith("```json")) rawJson = rawJson.Substring(7);
            if (rawJson.EndsWith("```")) rawJson = rawJson.Substring(0, rawJson.Length - 3);

            var jsonResult = JsonConvert.DeserializeObject<dynamic>(rawJson.Trim());

            var result = new GeneratedLocationResult
            {
                LocationName = jsonResult.location_name,
                Lore = jsonResult.lore
            };

            // 2. Math Topology (Algorithmically assigning Rect coordinates)
            float startX = 100f;
            float startY = 100f;
            float spacing = 200f;

            foreach (var bJson in jsonResult.buildings)
            {
                var building = new GeneratedBuilding
                {
                    Name = bJson.name,
                    Type = bJson.type,
                    PosX = startX,
                    PosY = startY
                };
                result.Buildings.Add(building);

                startX += spacing;
                if (startX > 800)
                {
                    startX = 100f;
                    startY += spacing;
                }
            }

            foreach (var nJson in jsonResult.npcs)
            {
                var npc = new GeneratedNpc
                {
                    Id = nJson.id,
                    Name = nJson.name,
                    Profession = nJson.profession,
                    InitialGoapGoal = nJson.initial_goap_goal
                };
                foreach(var invId in nJson.inventory_ids)
                {
                    npc.InventoryIds.Add((string)invId);
                }
                result.Npcs.Add(npc);
            }

            ServiceLocator.Logger.LogInfo($"WorldBuilder: Successfully generated '{result.LocationName}' with {result.Buildings.Count} buildings and {result.Npcs.Count} NPCs.");
            return result;
        }
        catch (Exception ex)
        {
            ServiceLocator.Logger.LogError($"WorldBuilder: Failed to generate location. {ex.Message}");
            throw;
        }
    }
}
