using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using DifferentWay.Systems.Models;

namespace DifferentWay.AI;

public partial class LLMClient : RefCounted
{
    private readonly System.Net.Http.HttpClient _httpClient;
    private readonly ErrorManager _errorManager;
    private readonly GraphValidator _validator;
    private string _apiKey = string.Empty;
    private string _apiUrl = "https://api.openai.com/v1/chat/completions";

    private CancellationTokenSource? _cts;

    [Signal]
    public delegate void AiResponseReceivedEventHandler(string jsonResponse);

    // For Godot to instantiate RefCounted objects cleanly from script, an empty constructor is needed
    public LLMClient()
    {
        _httpClient = new System.Net.Http.HttpClient();
        _errorManager = new ErrorManager();
        _validator = new GraphValidator();
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public LLMClient(ErrorManager errorManager, GraphValidator validator) : this()
    {
        _errorManager = errorManager;
        _validator = validator;
    }

    public void SetCredentials(string key, string url)
    {
        _apiKey = key;
        _apiUrl = url;
    }

    // Exposed for GDScript Calling
    public void RequestPromptAsync(string promptString)
    {
        // Add chat to context before sending
        var simulation = Godot.Engine.GetMainLoop() as Godot.SceneTree;
        var simNode = simulation?.Root.GetNodeOrNull<DifferentWay.Core.Simulation>("/root/Simulation");
        var timeManager = simulation?.Root.GetNodeOrNull<DifferentWay.Core.TimeManager>("/root/TimeManager");

        string fullPrompt = promptString;
        if (simNode != null && timeManager != null)
        {
            var liveState = simNode.GetLiveState();
            liveState.Context.AddChatHistory("Player: " + promptString);
            liveState.Context.UpdateWorldState(timeManager, liveState.PlayerStats);
            liveState.Context.FetchRagMemories(promptString);
            fullPrompt = liveState.PromptBuilder.BuildFinalPrompt();
        }

        // Recreate token
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        // Fire and forget from GDScript perspective
        _ = SendPromptAsyncHelper(fullPrompt, _cts.Token);
    }

    private async Task SendPromptAsyncHelper(string promptString, CancellationToken token)
    {
        // Use promptString (which is now the fully built context) to send to LLM
        var response = await SendPromptAsync(promptString, token);
        if (response != null)
        {
            string json = JsonSerializer.Serialize(response);
            CallDeferred(MethodName.EmitAiResponse, json);
        }
    }

    private void EmitAiResponse(string json)
    {
        EmitSignal(SignalName.AiResponseReceived, json);
    }

    public void CancelPendingRequests()
    {
        if (_cts != null && !_cts.IsCancellationRequested)
        {
            _cts.Cancel();
            GD.Print("LLM API request cancelled via GDScript.");
        }
    }

    // 7.2: Task Cancellation (CancellationToken) to prevent crash if UI is closed
    public async Task<AIResponse?> SendPromptAsync(string promptString, CancellationToken token)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            // MOCK FOR SANDBOX: If no API key, simulate a successful AI response after 1 second
            DifferentWay.Core.GameLogger.Log("LLMClient: No API key. Returning simulated AI response.");
            await Task.Delay(1000, token);

            var mockResponse = new AIResponse
            {
                Thoughts = "Игрок запросил квест. Я выдам задание на убийство волков.",
                SpokenText = "Приветствую! В лесу развелось слишком много волков. Разберись с ними, и я щедро заплачу."
            };
            return mockResponse;
        }

        // Prepare standard JSON payload for an LLM (e.g. OpenAI format)
        var requestPayload = new
        {
            model = "gpt-4o", // Example model
            messages = new[]
            {
                new { role = "system", content = "You are a game AI returning JSON strictly adhering to the schema." },
                new { role = "user", content = promptString }
            }
        };

        string jsonPayload = JsonSerializer.Serialize(requestPayload);
        using var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        // Wrap the actual HTTP call in our ErrorManager for Exponential Backoff (Section 8)
        string jsonResponse = await _errorManager.ExecuteWithRetryAsync(async () =>
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, _apiUrl);
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");
            request.Content = content;

            using var response = await _httpClient.SendAsync(request, token);

            // This throws if 429 (Rate Limit) or 500 (Server Error)
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync(token);
        }, fallbackDbStub: "{\"thoughts\":\"[DB Stub]\", \"spoken_text\":\"...\"}");

        // Handle Fallback
        if (jsonResponse.Contains("[DB Stub]"))
        {
            return new AIResponse { Thoughts = "Fallback", SpokenText = "Я сейчас не могу говорить." };
        }

        // Parse LLM response JSON wrapper
        // Note: For a real LLM, you must parse the 'choices[0].message.content' first.
        try
        {
            using var doc = JsonDocument.Parse(jsonResponse);
            var root = doc.RootElement;
            string actualJson = root.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "{}";

            // Validate against the mathematical constraints of the Skeleton
            if (!_validator.ValidateAiResponse(actualJson))
            {
                TelemetryLogger.LogHallucination(promptString, actualJson, "Failed validation logic constraints.");
                return new AIResponse { Thoughts = "Error", SpokenText = "Произошла ошибка в логике." };
            }

            var aiResponse = JsonSerializer.Deserialize<AIResponse>(actualJson);
            return aiResponse;
        }
        catch (OperationCanceledException)
        {
            GD.Print("LLM API request cancelled by user/UI.");
            return null; // 7.2: Task Cancellation handling
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to parse AI response: {ex.Message}");
            TelemetryLogger.LogHallucination(promptString, jsonResponse, ex.Message);
            return null;
        }
    }
}
