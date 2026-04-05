using Godot;
using System;
using System.Threading;

public partial class Simulation : Node
{
    // Deterministic random generator for logic loops
    private Random _rng;
    private int _worldSeed = 1337; // Fixed seed for MVP testing

    // The current logical state
    private GameState _liveState;
    // The safely readable state for UI/Rendering (Double-Buffering pattern)
    private GameState _snapshotState;

    private ReaderWriterLockSlim _stateLock;

    // A placeholder DTO for state
    public struct GameState
    {
        public int TurnCount;
        public int GlobalEconomyHealth;
    }

    public override void _Ready()
    {
        _rng = new Random(_worldSeed);
        _liveState = new GameState { TurnCount = 0, GlobalEconomyHealth = 100 };
        _snapshotState = _liveState;
        _stateLock = new ReaderWriterLockSlim();

        GD.Print($"[Simulation] Thread-Safe Determinism initialized with Seed: {_worldSeed}");
    }

    public void ProcessTurnBackground()
    {
        // This method simulates a heavy background task representing macroeconomic generation
        _stateLock.EnterWriteLock();
        try
        {
            _liveState.TurnCount++;
            _liveState.GlobalEconomyHealth += _rng.Next(-5, 6); // Random fluctuation
        }
        finally
        {
            _stateLock.ExitWriteLock();
        }
        CreateSnapshot();
    }

    private void CreateSnapshot()
    {
        _stateLock.EnterReadLock();
        try
        {
            _snapshotState = _liveState;
        }
        finally
        {
            _stateLock.ExitReadLock();
        }
    }

    // UI calls this method to read safe data without blocking background sim
    public GameState GetSnapshot()
    {
        _stateLock.EnterReadLock();
        try
        {
            return _snapshotState;
        }
        finally
        {
            _stateLock.ExitReadLock();
        }
    }
}
