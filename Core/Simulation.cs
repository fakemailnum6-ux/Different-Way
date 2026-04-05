using System.Threading;
using Godot;

namespace DifferentWay.Core;

public class GameState
{
    // Properties of game state
}

public partial class Simulation : Node
{
    private ReaderWriterLockSlim _stateLock = new ReaderWriterLockSlim();

    public GameState GameState_Live { get; private set; } = new GameState();
    public GameState GameState_Snapshot { get; private set; } = new GameState();

    public void UpdateSnapshot()
    {
        _stateLock.EnterWriteLock();
        try
        {
            // Shallow copy or deep copy of state for UI to read from
            GameState_Snapshot = GameState_Live;
        }
        finally
        {
            _stateLock.ExitWriteLock();
        }
    }
}
