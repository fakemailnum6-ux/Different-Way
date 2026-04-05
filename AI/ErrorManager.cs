using System;
using System.Threading.Tasks;
using Godot;

namespace DifferentWay.AI;

public class ErrorManager
{
    private int _circuitBreakerCount = 0;
    private const int MaxRetries = 3;

    // 8. ErrorManager: Circuit Breaker. Exponential Backoff. Максимум 3 попытки, затем Fallback-заглушка из БД.
    public async Task<string> ExecuteWithRetryAsync(Func<Task<string>> apiCall, string fallbackDbStub)
    {
        if (_circuitBreakerCount >= MaxRetries)
        {
            GD.PrintErr("Circuit Breaker OPEN. Falling back to DB stub.");
            return fallbackDbStub;
        }

        int delay = 1000; // Base delay in ms

        for (int i = 0; i < MaxRetries; i++)
        {
            try
            {
                string result = await apiCall();
                _circuitBreakerCount = 0; // Reset on success
                return result;
            }
            catch (Exception ex)
            {
                // Typical checks for 429/500 would happen here via HttpRequestException
                GD.PrintErr($"API Error: {ex.Message}. Retrying in {delay}ms...");
                _circuitBreakerCount++;

                await Task.Delay(delay);
                delay *= 2; // Exponential Backoff
            }
        }

        GD.PrintErr("Max retries reached. Opening Circuit Breaker and falling back to DB stub.");
        return fallbackDbStub;
    }

    public void ResetCircuitBreaker()
    {
        _circuitBreakerCount = 0;
    }
}
