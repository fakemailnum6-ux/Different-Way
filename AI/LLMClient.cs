using System.Threading.Tasks;

namespace DifferentWay.AI
{
    public class LLMClient
    {
        public async Task<string> GetResponseAsync(string prompt)
        {
            // Mocking an AI response delay
            await Task.Delay(100);

            if (prompt.Contains("Barnaby"))
            {
                return "Welcome to the Sleepy Dragon, traveler! I'm Barnaby. Looking for a room, or just a pint of our finest ale?";
            }

            return "The NPC stares at you blankly.";
        }
    }
}
