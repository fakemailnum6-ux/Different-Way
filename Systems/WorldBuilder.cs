using Godot;
using System;
using System.Collections.Generic;

namespace DifferentWay.Systems
{
    public class LocationParams
    {
        public string Biome { get; set; }
        public string SettlementType { get; set; }
        public string Faction { get; set; }
        public int PlayerLevel { get; set; }
        public string NarrativeContext { get; set; }
    }

    public partial class WorldBuilder : Node
    {
        private DifferentWay.AI.LLMClient _llmClient;
        private DifferentWay.Core.EventBus _eventBus;

        public override void _Ready()
        {
            _llmClient = GetNodeOrNull<DifferentWay.AI.LLMClient>("/root/LLMClient");
            _eventBus = GetNodeOrNull<DifferentWay.Core.EventBus>("/root/EventBus");
        }

        // Returns a task to asynchronously generate the settlement via LLM
        public async System.Threading.Tasks.Task<SettlementNode> GenerateLocationAsync(LocationParams p)
        {
            _eventBus?.EmitLogMessage("INFO", "Генерация новой локации через ИИ...");

            string prompt = $"Создай поселение. Биом: {p.Biome}, Тип: {p.SettlementType}. Уровень: {p.PlayerLevel}. Лор: {p.NarrativeContext}. " +
                            "Опиши его название (Name), описание (Description) и двух NPC (Name, Role, GoapGoal). " +
                            "ВЕРНИ ТОЛЬКО СТРОГИЙ JSON без markdown. Формат: { \"Name\": \"\", \"Description\": \"\", \"NPCs\": [ {\"Name\": \"\", \"Role\": \"\", \"GoapGoal\": \"\"} ] }";

            string jsonResponse = null;
            if (_llmClient != null)
            {
                jsonResponse = await _llmClient.SendRequest(prompt, "Ты - генератор мира. Верни валидный JSON по заданной схеме.");
            }

            var settlement = new SettlementNode
            {
                Id = "town_" + Guid.NewGuid().ToString().Substring(0, 8),
                Biome = p.Biome
            };

            // Attempt to parse AI response. Fallback if null or failed.
            if (!string.IsNullOrEmpty(jsonResponse))
            {
                try
                {
                    // Clean markdown blocks if AI ignored instructions
                    int start = jsonResponse.IndexOf('{');
                    int end = jsonResponse.LastIndexOf('}');
                    if (start >= 0 && end > start)
                    {
                        string cleanJson = jsonResponse.Substring(start, end - start + 1);
                        using var doc = System.Text.Json.JsonDocument.Parse(cleanJson);
                        var root = doc.RootElement;

                        settlement.Name = root.TryGetProperty("Name", out var n) ? n.GetString() : "Неизвестное поселение";
                        settlement.Description = root.TryGetProperty("Description", out var d) ? d.GetString() : "ИИ не смог описать это место.";

                        // Parse NPCs
                        var tavern = new BuildingNode { Id = "b1", Type = "Tavern", Position = new Vector2(100, 100) };
                        var farm = new BuildingNode { Id = "b2", Type = "Farm", Position = new Vector2(200, 300) };

                        if (root.TryGetProperty("NPCs", out var npcs) && npcs.ValueKind == System.Text.Json.JsonValueKind.Array)
                        {
                            int npcIdx = 1;
                            foreach (var npcEl in npcs.EnumerateArray())
                            {
                                var npc = new NpcNode
                                {
                                    Id = $"npc_{npcIdx}",
                                    Name = npcEl.TryGetProperty("Name", out var nn) ? nn.GetString() : "Незнакомец",
                                    Role = npcEl.TryGetProperty("Role", out var nr) ? nr.GetString() : "Житель",
                                    GoapGoal = npcEl.TryGetProperty("GoapGoal", out var ng) ? ng.GetString() : "Бродит без цели"
                                };

                                if (npcIdx == 1) tavern.Npcs.Add(npc);
                                else farm.Npcs.Add(npc);

                                npcIdx++;
                            }
                        }

                        settlement.Buildings.Add(tavern);
                        settlement.Buildings.Add(farm);

                        // Roads
                        settlement.Roads["b1"] = new List<string> { "b2" };
                        settlement.Roads["b2"] = new List<string> { "b1" };
                    }
                }
                catch (Exception e)
                {
                    _eventBus?.EmitLogMessage("ERROR", $"Ошибка парсинга генератора мира: {e.Message}");
                    GenerateFallback(settlement);
                }
            }
            else
            {
                 GenerateFallback(settlement);
            }

            _eventBus?.EmitLogMessage("INFO", $"Генерация завершена: {settlement.Name}");
            return settlement;
        }

        private void GenerateFallback(SettlementNode settlement)
        {
            settlement.Name = "Забытый Перекресток (Fallback)";
            settlement.Description = "Маленькая деревня, сгенерированная из-за ошибки ИИ.";

            var tavern = new BuildingNode { Id = "b1", Type = "Tavern", Position = new Vector2(100, 100) };
            var smithy = new BuildingNode { Id = "b2", Type = "Smithy", Position = new Vector2(300, 150) };

            tavern.Npcs.Add(new NpcNode { Id = "npc_1", Name = "Трактирщик Боб", Role = "Merchant", GoapGoal = "Ждет клиентов" });

            settlement.Buildings.Add(tavern);
            settlement.Buildings.Add(smithy);

            settlement.Roads["b1"] = new List<string> { "b2" };
            settlement.Roads["b2"] = new List<string> { "b1" };
        }
    }
}
