using System;
using System.Net.Http;
using System.Threading.Tasks;
using Godot;
using DifferentWay.AI;

namespace DifferentWay.Tests.Mocks
{
    public class MockLLMServer
    {
        public async Task TestRetryLogicAsync()
        {
            var client = new LLMClient();
            var errorManager = new ErrorManager(client);

            GD.Print("Testing retry logic against failing client...");

            // This will fail with the dummy client, triggering retries
            string result = await errorManager.ExecuteWithRetryAsync("Test prompt");

            GD.Print($"Retry test result: {result}");
        }
    }
}
