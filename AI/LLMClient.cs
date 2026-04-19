using Godot;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace DifferentWay.AI
{
    public partial class LLMClient : Node
    {
        private System.Net.Http.HttpClient _httpClient;
        private string _apiToken;

        public override void _Ready()
        {
            _apiToken = System.Environment.GetEnvironmentVariable("PUTER_API_TOKEN") ?? "MISSING_TOKEN";
            _httpClient = new System.Net.Http.HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiToken}");
        }

        public async Task<string> SendRequest(string promptPayload, string systemPrompt)
        {
            var requestBody = new {
                model = "grok-4-fast",
                stream = false,
                messages = new[] {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = promptPayload }
                }
            };

            string jsonBody = JsonSerializer.Serialize(requestBody);
            var content = new System.Net.Http.StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");

            try {
                var response = await _httpClient.PostAsync("https://api.puter.com/puterai/openai/v1/chat/completions", content);
                response.EnsureSuccessStatusCode();
                string rawJson = await response.Content.ReadAsStringAsync();

                // Extract content from OpenAI envelope
                using (JsonDocument doc = JsonDocument.Parse(rawJson))
                {
                    JsonElement root = doc.RootElement;
                    if (root.TryGetProperty("choices", out JsonElement choices) && choices.GetArrayLength() > 0)
                    {
                        var firstChoice = choices[0];
                        if (firstChoice.TryGetProperty("message", out JsonElement message))
                        {
                            if (message.TryGetProperty("content", out JsonElement contentElement))
                            {
                                return contentElement.GetString();
                            }
                        }
                    }
                }

                return null; // Unexpected envelope structure
            } catch (Exception e) {
                GD.PrintErr($"LLM Request failed: {e.Message}");
                return null;
            }
        }
    }
}
