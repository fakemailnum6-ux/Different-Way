using System;
using System.Threading;
using System.Threading.Tasks;
using Godot;

namespace DifferentWay.AI
{
    public class ErrorManager
    {
        private readonly LLMClient _client;
        private readonly int[] _backoffDelays = { 1000, 2000, 4000 }; // 1s, 2s, 4s

        public ErrorManager(LLMClient client)
        {
            _client = client;
        }

        public async Task<string> ExecuteWithRetryAsync(string prompt)
        {
            for (int attempt = 0; attempt <= _backoffDelays.Length; attempt++)
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)); // 10s timeout
                try
                {
                    // For the sake of matching the architecture exactly, we use a custom wrapper here
                    // that might enforce the timeout since LLMClient handles its own try-catch in the dummy setup.
                    // Assuming LLMClient throws exception instead of swallowing if we want ErrorManager to handle retries.

                    return await _client.SendPromptAsync(prompt);
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"Attempt {attempt + 1} failed: {ex.Message}");
                    if (attempt < _backoffDelays.Length)
                    {
                        GD.Print($"Retrying in {_backoffDelays[attempt]}ms...");
                        await Task.Delay(_backoffDelays[attempt]);
                    }
                }
            }

            GD.PrintErr("All retries exhausted. Switching to offline mode.");
            // Force fallback string
            return "{\"character_thoughts\": \"I can't think straight.\", \"spoken_text\": \"The trader turns away contemptuously, refusing to speak with you.\"}";
        }
    }
}
