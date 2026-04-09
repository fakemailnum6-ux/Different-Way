using Godot;
using System;
using System.Threading;
using System.Threading.Tasks;

public enum CombatState
{
    Init,
    PlayerTurn,
    EnemyTurn,
    Resolve,
    End
}

public partial class CombatManager : RefCounted
{
    public CombatState CurrentState { get; private set; }

    // Example Combat entity tracker
    public bool IsPlayerTurn { get; private set; }
    public bool AutoDefenseTriggered { get; private set; }

    private CancellationTokenSource _turnTimerCts;
    private const int PlayerTurnDurationMs = 10000;

    public void StartCombat()
    {
        TransitionToState(CombatState.Init);
    }

    private void TransitionToState(CombatState newState)
    {
        CurrentState = newState;
        GD.Print($"[CombatManager] Entered {newState}");

        switch (newState)
        {
            case CombatState.Init:
                // 1. Calculate Initiative (Init phase)
                // 2. Sort Queue
                // 3. Move to first entity
                IsPlayerTurn = true; // Hardcoded for timer demonstration
                TransitionToState(IsPlayerTurn ? CombatState.PlayerTurn : CombatState.EnemyTurn);
                break;

            case CombatState.PlayerTurn:
                AutoDefenseTriggered = false;
                StartPlayerTurnTimerAsync();
                break;

            case CombatState.EnemyTurn:
                // Instant AI logic (Targeting, Cooldowns)
                TransitionToState(CombatState.Resolve);
                break;

            case CombatState.Resolve:
                // Math phase (DamageCalculator, Evasion, DoT)
                if (AutoDefenseTriggered)
                {
                    GD.Print("[CombatManager] Resolving with 'Auto-Defense' (Timer Expired).");
                }

                // End combat for demonstration
                TransitionToState(CombatState.End);
                break;

            case CombatState.End:
                // Signal GameStateMachine to close UI
                var eventBus = ServiceLocator.EventBus;
                eventBus?.EmitSignal("RequestStateChange", (int)GameState.Exploration);
                break;
        }
    }

    private async void StartPlayerTurnTimerAsync()
    {
        // Cancel any previous timer
        _turnTimerCts?.Cancel();
        _turnTimerCts = new CancellationTokenSource();

        try
        {
            // 10 Second strict timer
            await Task.Delay(PlayerTurnDurationMs, _turnTimerCts.Token);

            // Timer expired without user input
            if (CurrentState == CombatState.PlayerTurn)
            {
                GD.Print("[CombatManager] Time's up! Auto-skipping turn.");
                AutoDefenseTriggered = true;
                TransitionToState(CombatState.Resolve);
            }
        }
        catch (TaskCanceledException)
        {
            // User submitted an action before 10s elapsed
            GD.Print("[CombatManager] Timer cancelled early via Player Input.");
        }
    }

    /// <summary>
    /// Invoked by UI Command when player clicks Attack/Skill/Potion
    /// </summary>
    public void SubmitPlayerAction()
    {
        if (CurrentState != CombatState.PlayerTurn) return;

        // Cancel the 10s timer
        _turnTimerCts?.Cancel();

        // Progress to Math
        TransitionToState(CombatState.Resolve);
    }
}
