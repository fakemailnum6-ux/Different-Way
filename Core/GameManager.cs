using Godot;
using System;
using System.Threading.Tasks;

public partial class GameManager : Node
{
    public override void _Ready()
    {
        ServiceLocator.Initialize(GetTree());

        // Listen to state changes to load data when Loading
        ServiceLocator.EventBus.StateTransitioned += OnStateTransitioned;

        // Initial state is loading
        CallDeferred(nameof(InitDataAsync));
    }

    private async void InitDataAsync()
    {
        await ServiceLocator.DataManager.InitializeAsync();

        // After loading is complete, transition out of Loading state
        ServiceLocator.EventBus.Emit("RequestStateChange", (int)GameState.Exploration);
    }

    private void OnStateTransitioned(int newStateEnum)
    {
        GameState state = (GameState)newStateEnum;
        if (state == GameState.Loading)
        {
            _ = ServiceLocator.DataManager.InitializeAsync();
        }
    }
}
