using Godot;
using System.Threading;

namespace DifferentWay.Core
{
    public class GameState
    {
        public int NpcCount { get; set; } = 10;
        // Other state variables would go here
    }

    public partial class Simulation : Node
    {
        public int WorldSeed { get; private set; } = 42; // Fixed seed for determinism

        private GameState _gameStateLive = new GameState();
        private GameState _gameStateSnapshot = new GameState();

        private ReaderWriterLockSlim _stateLock = new ReaderWriterLockSlim();

        public override void _Process(double delta)
        {
            // Simulate background logic (in a real app, this might be a separate thread loop)
            UpdateLiveState(delta);

            // Once per tick/interval, copy to snapshot
            UpdateSnapshot();
        }

        private void UpdateLiveState(double delta)
        {
            // Example live state update logic
            _gameStateLive.NpcCount = 10 + (int)GD.Randi() % 5;
        }

        private void UpdateSnapshot()
        {
            _stateLock.EnterWriteLock();
            try
            {
                // Deep copy (simplified for this example)
                _gameStateSnapshot.NpcCount = _gameStateLive.NpcCount;
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }
        }

        public GameState GetSnapshot()
        {
            _stateLock.EnterReadLock();
            try
            {
                // Return a copy or just read access depending on performance needs
                // Returning a new copy to ensure strict isolation if callers modify it
                return new GameState { NpcCount = _gameStateSnapshot.NpcCount };
            }
            finally
            {
                _stateLock.ExitReadLock();
            }
        }
    }
}
