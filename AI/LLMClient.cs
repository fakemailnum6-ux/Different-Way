using System;
using System.Net.Http;
using System.Threading.Tasks;
using Godot;

namespace DifferentWay.AI
{
    public class LLMClient
    {
        private readonly System.Net.Http.HttpClient _httpClient;
        public bool IsOffline { get; private set; } = false;

        public LLMClient()
        {
            _httpClient = new System.Net.Http.HttpClient();
        }

        public async Task<string> SendPromptAsync(string prompt)
        {
            if (IsOffline)
            {
                return GetOfflineFallback();
            }

            try
            {
                // Dummy API call
                var content = new StringContent(prompt);
                var response = await _httpClient.PostAsync("https://dummy-llm-api.example.com/generate", content);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                GD.PrintErr($"LLM API Request Failed: {ex.Message}");
                IsOffline = true;
                return GetOfflineFallback();
            }
            catch (TaskCanceledException)
            {
                GD.PrintErr("LLM API Request Timed out.");
                IsOffline = true;
                return GetOfflineFallback();
            }
        }

        private string GetOfflineFallback()
        {
            return "{\"character_thoughts\": \"Something is wrong with the connection...\", \"spoken_text\": \"The trader turns away contemptuously, refusing to speak with you.\"}";
        }

        public void ResetOfflineMode()
        {
            IsOffline = false;
        }
    }
}
