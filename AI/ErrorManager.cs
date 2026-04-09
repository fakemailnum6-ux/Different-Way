using System.Threading;
using Godot;
using System;
using System.Threading.Tasks;

public partial class ErrorManager : RefCounted
{
    private const int MaxRetries = 3;
    private const int BaseDelayMs = 1000;

    private int _circuitBreakerFailures = 0;
    private const int CircuitBreakerThreshold = 5;
    private DateTime _circuitBreakerCooldown = DateTime.MinValue;

    public async Task<string> ExecuteWithExponentialBackoffAsync(Func<Task<string>> action, CancellationToken token = default)
    {
        if (IsCircuitBroken())
        {
            ServiceLocator.Logger.LogWarning("ErrorManager: Circuit breaker is OPEN. Returning fallback immediately.");
            return GetFallbackResponse();
        }

        int attempt = 0;
        while (attempt < MaxRetries)
        {
            try
            {
                string result = await action();
                // Success resets circuit breaker
                _circuitBreakerFailures = 0;
                return result;
            }
            catch (OperationCanceledException)
            {
                ServiceLocator.Logger.LogInfo("ErrorManager: Request cancelled by user.");
                throw;
            }
            catch (Exception ex)
            {
                attempt++;
                ServiceLocator.Logger.LogWarning($"ErrorManager: AI Request Failed (Attempt {attempt}/{MaxRetries}). Reason: {ex.Message}");

                if (attempt >= MaxRetries)
                {
                    _circuitBreakerFailures++;
                    if (_circuitBreakerFailures >= CircuitBreakerThreshold)
                    {
                        ServiceLocator.Logger.LogError("ErrorManager: Circuit breaker tripped! Suspending API calls for 60 seconds.");
                        _circuitBreakerCooldown = DateTime.Now.AddSeconds(60);
                    }
                    break;
                }

                // Exponential backoff: 1s, 2s, 4s...
                int delay = BaseDelayMs * (int)Math.Pow(2, attempt - 1);
                await Task.Delay(delay, token);
            }
        }

        return GetFallbackResponse();
    }

    private bool IsCircuitBroken()
    {
        return _circuitBreakerFailures >= CircuitBreakerThreshold && DateTime.Now < _circuitBreakerCooldown;
    }

    private string GetFallbackResponse()
    {
        string fallbackText = "The NPC remains silent.";

        if (ServiceLocator.LocalizationManager != null)
        {
            fallbackText = ServiceLocator.LocalizationManager.Translate("fallback_ai_error");
        }

        // Return a mock JSON schema to prevent parsing crashes down the line
        var mockResponse = new
        {
            thoughts = "System Error Fallback",
            spoken_text = fallbackText,
            action_triggers = new[] { new { type = "end_dialogue" } }
        };

        return Newtonsoft.Json.JsonConvert.SerializeObject(mockResponse);
    }
}
