using Godot;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public partial class LLMClient : Node
{
    [Signal]
    public delegate void AiResponseReceivedEventHandler(string jsonPayload);

    private const string ApiEndpoint = "https://api.puter.com/puterai/openai/v1/chat/completions";
    private readonly System.Net.Http.HttpClient _httpClient = new System.Net.Http.HttpClient();
    private ErrorManager _errorManager;

    public override void _Ready()
    {
        _errorManager = new ErrorManager();
        string apiKey = KeyManager.LoadApiKey();

        if (string.IsNullOrEmpty(apiKey))
        {
            ServiceLocator.Logger.LogError("LLMClient: API Key is missing. Please save it via KeyManager.");
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    /// <summary>
    /// Core REST logic using ErrorManager for 3x Exponential Backoff / Circuit Breaker
    /// Includes CancellationToken support for UI cancellation (Arc.md Section 7.4)
    /// </summary>
    public async Task<string> RequestPromptAsync(string systemPrompt, string userPrompt, CancellationToken token = default)
    {
        ServiceLocator.Logger.LogInfo("LLMClient: Sending REST request to Puter API...");

        string finalJson = await _errorManager.ExecuteWithExponentialBackoffAsync(async () =>
        {
            return await PerformRestCallAsync(systemPrompt, userPrompt, token);
        }, token);

        // EventBus broadcasts the raw string JSON to listeners (like GraphValidator or GDScript map)
        EmitSignal(SignalName.AiResponseReceived, finalJson);
        return finalJson;
    }

    private async Task<string> PerformRestCallAsync(string systemPrompt, string userPrompt, CancellationToken token)
    {
        var requestBody = new
        {
            model = "grok-4-fast",
            stream = false,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            }
        };

        string jsonBody = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.PostAsync(ApiEndpoint, content, token);

        response.EnsureSuccessStatusCode();

        string responseJson = await response.Content.ReadAsStringAsync();

        // Arc.md Section 5.1: Path is data.choices[0].message.content
        JObject parsedResponse = JObject.Parse(responseJson);
        string messageContent = (string)parsedResponse["choices"]?[0]?["message"]?["content"];

        if (string.IsNullOrEmpty(messageContent))
        {
            throw new Exception("LLMClient: API returned 200 OK, but choices array was missing or malformed.");
        }

        return messageContent;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _httpClient?.Dispose();
        }
        base.Dispose(disposing);
    }
}
