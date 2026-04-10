using Godot;
using System;

public enum GameState
{
    Loading,
    Exploration,
    Dialogue,
    Combat,
    Menu
}

public partial class GameStateMachine : Node
{
    public GameState CurrentState { get; private set; } = GameState.Loading;
    public int GetCurrentState() => (int)CurrentState;

    public override void _Ready()
    {
        var eventBus = GetNodeOrNull<EventBus>("/root/EventBus");
        if (eventBus != null)
        {
            eventBus.RequestStateChange += OnRequestStateChange;
        }
    }

    private void OnRequestStateChange(int stateEnum)
    {
        GameState newState = (GameState)stateEnum;
        if (CurrentState == newState) return;

        CurrentState = newState;

        var eventBus = GetNodeOrNull<EventBus>("/root/EventBus");
        eventBus?.EmitSignal(EventBus.SignalName.StateTransitioned, (int)CurrentState);

        GD.Print($"[GameStateMachine] Transitioned to {CurrentState}");
    }
}
