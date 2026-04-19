using Godot;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

public partial class Simulation : Node
{
    private GameState_Live _liveState;
    private GameState_Snapshot _currentSnapshot;

    private ConcurrentQueue<ICommand> _commandQueue = new ConcurrentQueue<ICommand>();

    private CancellationTokenSource _cancellationTokenSource;
    private Task _simulationTask;

    // Target 60 Ticks Per Second (16.6ms per tick)
    private const int TargetMillisecondsPerTick = 16;

    public override void _Ready()
    {
        _liveState = new GameState_Live();
        _currentSnapshot = _liveState.CreateSnapshot();

        _cancellationTokenSource = new CancellationTokenSource();
        _simulationTask = Task.Run(() => SimulationLoop(_cancellationTokenSource.Token));

        GD.Print("Simulation: Background thread started at 60 TPS.");
    }

    /// <summary>
    /// Public API for UI to safely retrieve the latest immutable state
    /// without locking the simulation thread.
    /// </summary>
    public GameState_Snapshot GetLatestSnapshot()
    {
        return Interlocked.CompareExchange(ref _currentSnapshot, null, null);
    }

    /// <summary>
    /// Enqueues a command to be executed in the next simulation tick.
    /// </summary>
    public void QueueCommand(ICommand command)
    {
        _commandQueue.Enqueue(command);
    }

    private async Task SimulationLoop(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                long startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                bool stateChanged = false;

                // 1. Process all pending commands
                while (_commandQueue.TryDequeue(out ICommand command))
                {
                    bool commandChangedState = command.Execute(_liveState);
                    if (commandChangedState)
                    {
                        stateChanged = true;
                    }
                }

                // 2. Publish new snapshot if mutation occurred
                if (stateChanged)
                {
                    _liveState.StateVersion++;

                    var newSnapshot = _liveState.CreateSnapshot();
                    Interlocked.Exchange(ref _currentSnapshot, newSnapshot);

                    // The UI thread can optionally be notified via EventBus,
                    // though polling Version is standard pattern described in Arc.md
                    // ServiceLocator.EventBus.EmitSignal("OnStateMutated");
                }

                // 3. Throttle loop to 60 TPS
                long elapsed = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - startTime;
                int delay = TargetMillisecondsPerTick - (int)elapsed;

                if (delay > 0)
                {
                    await Task.Delay(delay, token);
                }
                else
                {
                    // Prevent thread starvation if processing took too long
                    await Task.Yield();
                }
            }
        }
        catch (TaskCanceledException)
        {
            GD.Print("Simulation: Task cancelled successfully.");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Simulation: Fatal error in background thread: {ex.Message}");
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }
        base.Dispose(disposing);
    }
}
