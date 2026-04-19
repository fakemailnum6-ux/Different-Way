using Godot;
using System.Collections.Generic;
using DifferentWay.Core;

namespace DifferentWay.Systems
{
    public enum QuestStatus
    {
        Active,
        Completed,
        Failed
    }

    public class QuestObjective
    {
        public string Type { get; set; } // "kill_mob", "fetch_item"
        public string TargetId { get; set; } // e.g. "wolf_01"
        public int CurrentAmount { get; set; } = 0;
        public int RequiredAmount { get; set; }
    }

    public class QuestNode
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public QuestStatus Status { get; set; } = QuestStatus.Active;
        public List<QuestObjective> Objectives { get; set; } = new List<QuestObjective>();
        public int GoldReward { get; set; }
    }

    public partial class QuestManager : Node
    {
        private EventBus _eventBus;
        private List<QuestNode> _activeQuests = new List<QuestNode>();

        private DifferentWay.AI.LLMClient _llmClient;

        public override void _Ready()
        {
            _eventBus = GetNodeOrNull<EventBus>("/root/EventBus");
            _llmClient = GetNodeOrNull<DifferentWay.AI.LLMClient>("/root/LLMClient");
        }

        public async System.Threading.Tasks.Task AddQuestAsync(QuestNode mathSkeleton)
        {
            // Convert skeleton into a prompt
            string objDesc = "";
            foreach (var o in mathSkeleton.Objectives)
            {
                objDesc += $"{o.Type} {o.TargetId} ({o.RequiredAmount}x). ";
            }

            string prompt = $"Сгенерируй название (Title) и интересное лорное описание (Description) для этого математического квеста: {objDesc}. " +
                            $"Награда: {mathSkeleton.GoldReward} золота. " +
                            "ВЕРНИ ТОЛЬКО СТРОГИЙ JSON без markdown. Формат: { \"Title\": \"\", \"Description\": \"\" }";

            if (_llmClient != null)
            {
                string jsonResponse = await _llmClient.SendRequest(prompt, "Ты - генератор квестов.");

                if (!string.IsNullOrEmpty(jsonResponse))
                {
                    try
                    {
                        int start = jsonResponse.IndexOf('{');
                        int end = jsonResponse.LastIndexOf('}');
                        if (start >= 0 && end > start)
                        {
                            string cleanJson = jsonResponse.Substring(start, end - start + 1);
                            using var doc = System.Text.Json.JsonDocument.Parse(cleanJson);
                            var root = doc.RootElement;

                            mathSkeleton.Title = root.TryGetProperty("Title", out var t) ? t.GetString() : "Случайный квест";
                            mathSkeleton.Description = root.TryGetProperty("Description", out var d) ? d.GetString() : "Описание не найдено.";
                        }
                    }
                    catch (System.Exception e)
                    {
                        _eventBus?.EmitLogMessage("ERROR", $"Ошибка парсинга квеста от ИИ: {e.Message}");
                        mathSkeleton.Title = "Охота (Скелет)";
                        mathSkeleton.Description = "Убить цель. (Сбой ИИ)";
                    }
                }
            }
            else
            {
                mathSkeleton.Title = "Охота (Скелет)";
                mathSkeleton.Description = "Убить цель.";
            }

            _activeQuests.Add(mathSkeleton);
            _eventBus?.EmitLogMessage("INFO", $"Новое задание добавлено в журнал: {mathSkeleton.Title}");
        }

        public void AddQuest(QuestNode quest)
        {
             // Fallback for non-async / immediate testing
            _activeQuests.Add(quest);
            _eventBus?.EmitLogMessage("INFO", $"Новое задание: {quest.Title}");
        }

        public void ProcessKill(string mobId)
        {
            foreach (var q in _activeQuests)
            {
                if (q.Status != QuestStatus.Active) continue;

                foreach (var obj in q.Objectives)
                {
                    if (obj.Type == "kill_mob" && obj.TargetId == mobId)
                    {
                        if (obj.CurrentAmount < obj.RequiredAmount)
                        {
                            obj.CurrentAmount++;
                            _eventBus?.EmitLogMessage("INFO", $"Прогресс: {obj.CurrentAmount}/{obj.RequiredAmount} убито {mobId}");

                            CheckQuestCompletion(q);
                        }
                    }
                }
            }
        }

        private void CheckQuestCompletion(QuestNode quest)
        {
            foreach (var obj in quest.Objectives)
            {
                if (obj.CurrentAmount < obj.RequiredAmount) return; // Still running
            }

            quest.Status = QuestStatus.Completed;
            _eventBus?.EmitLogMessage("INFO", $"Задание выполнено: {quest.Title}");

            // Give reward logic
            // E.g. playerGold += quest.GoldReward;
        }

        public List<QuestNode> GetQuests() => _activeQuests;
    }
}
