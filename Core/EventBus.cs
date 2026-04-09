using Godot;
using System;

namespace DifferentWay.Core
{
    public partial class EventBus : Node
    {
        // Define global signals as C# events
        public delegate void GameStateTransitionEventHandler(string newState);
        public event GameStateTransitionEventHandler OnGameStateTransition;

        public delegate void HealthChangedEventHandler(string entityId, int newHealth, int maxHealth);
        public event HealthChangedEventHandler OnHealthChanged;

        public delegate void LogMessageEventHandler(string level, string message);
        public event LogMessageEventHandler OnLogMessage;

        public void EmitGameStateTransition(string newState)
        {
            OnGameStateTransition?.Invoke(newState);
        }

        public void EmitHealthChanged(string entityId, int newHealth, int maxHealth)
        {
            OnHealthChanged?.Invoke(entityId, newHealth, maxHealth);
        }

        public void EmitLogMessage(string level, string message)
        {
            OnLogMessage?.Invoke(level, message);
        }
    }
}
