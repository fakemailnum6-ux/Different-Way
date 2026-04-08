using Godot;
using System;
using System.Text.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace DifferentWay.AI
{
    public partial class LLMClient : Node
    {
        private HttpClient _httpClient;
        private string _apiToken;

        public override void _Ready()
        {
            _apiToken = Environment.GetEnvironmentVariable("PUTER_API_TOKEN") ?? "MISSING_TOKEN";
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiToken}");
        }

        public async Task<string> SendRequest(string promptPayload)
        {
            var requestBody = new {
                model = "grok-4-fast",
                stream = false,
                messages = new[] {
                    new { role = "system", content = "Твоя цель: отыгрывать NPC. Верни строго валидный JSON." },
                    new { role = "user", content = promptPayload }
                }
            };

            string jsonBody = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");

            try {
                var response = await _httpClient.PostAsync("https://api.puter.com/puterai/openai/v1/chat/completions", content);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            } catch (Exception e) {
                GD.PrintErr($"LLM Request failed: {e.Message}");
                return null;
            }
        }
    }
}
