using System.Collections.Generic;

namespace DifferentWay.Core
{
    // The Command Interface
    public interface ICommand
    {
        void Execute(GameState_Live state);
    }

    // The Mutable State
    public class GameState_Live
    {
        public int StateVersion { get; set; } = 0;
        public string PlayerLocationId { get; set; } = "zero_state";
        public Dictionary<string, int> PlayerInventory { get; set; } = new Dictionary<string, int>();
        public int PlayerGold { get; set; } = 0;
        public int PlayerHealth { get; set; } = 100;
        public int PlayerMaxHealth { get; set; } = 100;

        public void IncrementVersion()
        {
            StateVersion++;
        }
    }

    // The Immutable Snapshot (using C# records for shallow immutability, we copy dicts manually)
    public record GameState_Snapshot
    {
        public int Version { get; init; }
        public string PlayerLocationId { get; init; }
        public IReadOnlyDictionary<string, int> PlayerInventory { get; init; }
        public int PlayerGold { get; init; }
        public int PlayerHealth { get; init; }
        public int PlayerMaxHealth { get; init; }

        public GameState_Snapshot(GameState_Live liveState)
        {
            Version = liveState.StateVersion;
            PlayerLocationId = liveState.PlayerLocationId;
            PlayerGold = liveState.PlayerGold;
            PlayerHealth = liveState.PlayerHealth;
            PlayerMaxHealth = liveState.PlayerMaxHealth;

            // Deep copy collections
            PlayerInventory = new Dictionary<string, int>(liveState.PlayerInventory);
        }
    }
}
