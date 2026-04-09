using Godot;
using System.Collections.Generic;

namespace DifferentWay.AI
{
    public class ChatMessage
    {
        public string Role { get; set; } // "user" or "assistant" or "system"
        public string Content { get; set; }
    }

    public partial class ContextManager : Node
    {
        // FIFO Queue for memory per NPC. Keys are NPC IDs.
        private Dictionary<string, LinkedList<ChatMessage>> _npcHistories = new Dictionary<string, LinkedList<ChatMessage>>();
        private const int MAX_HISTORY = 10;

        public override void _Ready()
        {
        }

        public void AddMessage(string npcId, string role, string content)
        {
            if (!_npcHistories.ContainsKey(npcId))
            {
                _npcHistories[npcId] = new LinkedList<ChatMessage>();
            }

            var history = _npcHistories[npcId];
            history.AddLast(new ChatMessage { Role = role, Content = content });

            if (history.Count > MAX_HISTORY)
            {
                history.RemoveFirst();
            }
        }

        public List<ChatMessage> GetHistory(string npcId)
        {
            if (_npcHistories.ContainsKey(npcId))
            {
                return new List<ChatMessage>(_npcHistories[npcId]);
            }
            return new List<ChatMessage>();
        }

        public void ClearHistory(string npcId)
        {
             if (_npcHistories.ContainsKey(npcId))
            {
                _npcHistories[npcId].Clear();
            }
        }
    }
}
