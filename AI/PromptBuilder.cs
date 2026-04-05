using DifferentWay.Core;

namespace DifferentWay.AI
{
    public class PromptBuilder
    {
        public string BuildNpcPrompt(NPC npc, string playerMessage)
        {
            return $"You are {npc.Name}. {npc.Description}. The player says: \"{playerMessage}\"";
        }
    }
}
