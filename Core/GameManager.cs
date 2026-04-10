using Godot;
using System;
using System.Threading.Tasks;

public partial class GameManager : Node
{
    public override void _Ready()
    {
        ServiceLocator.Initialize(GetTree());

        // Initialize synchronous managers that rely on EventBus
        ServiceLocator.LocalizationManager.Initialize(ServiceLocator.EventBus);

        // Listen to state changes to load data when Loading
        ServiceLocator.EventBus.StateTransitioned += OnStateTransitioned;

        // Initial state is loading
        CallDeferred(nameof(InitDataAsync));
    }

    private async void InitDataAsync()
    {
        await ServiceLocator.DataManager.InitializeAsync();

        // Load Auto-save
        // Note: For actual integration, GameState_Live is inside Simulation.
        // We're invoking it securely via ServiceLocator in a real app,
        // but here we just trigger the initialization event.

        // After loading is complete, transition out of Loading state
        ServiceLocator.EventBus.EmitSignal(EventBus.SignalName.RequestStateChange, (int)GameState.Exploration);
    }

    private void OnStateTransitioned(int newStateEnum)
    {
        GameState state = (GameState)newStateEnum;

        if (state == GameState.Loading)
        {
            _ = ServiceLocator.DataManager.InitializeAsync();
        }
        else if (state == GameState.Exploration)
        {
            // Trigger auto-save whenever the player enters Exploration state (e.g. from Dialogue or Combat)
            // This is a placeholder since GameState_Live should be fetched from the Simulation context natively
            ServiceLocator.Logger.LogInfo("GameManager: Entered Exploration. Triggering Auto-Save mechanism.");

            // To prevent violating zero-reference architecture, typically we dispatch an ICommand to the queue:
            ServiceLocator.Simulation.QueueCommand(new SaveGameCommand());

            // For now, logging fulfills the trigger implementation conceptually
        }
    }
}
