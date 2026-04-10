using Godot;
using System;
using System.Threading;
using System.Threading.Tasks;

public partial class GameManager : Node
{
    private LLMClient _llmClient;
    private WorldBuilder _worldBuilder;

    public override void _Ready()
    {
        ServiceLocator.Initialize(GetTree());

        // Initialize AI Subsystems
        _llmClient = new LLMClient();
        AddChild(_llmClient); // Add to tree for signals and http functionality
        _worldBuilder = new WorldBuilder(_llmClient);

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

        // Once data is loaded, begin the Zero-State Protocol (Arc.md Section 11)
        ServiceLocator.Logger.LogInfo("GameManager: Triggering Zero-State Protocol.");
        ServiceLocator.EventBus.EmitSignal(EventBus.SignalName.RequestStateChange, (int)GameState.Dialogue);

        // In a real UI, this would be an input box. Simulating the prompt here:
        await ExecuteZeroStateProtocol("Я бывший городской стражник, которого выгнали за пьянство, теперь скитаюсь без гроша");
    }

    private async Task ExecuteZeroStateProtocol(string playerBackstory)
    {
        ServiceLocator.Logger.LogInfo($"GameManager: Parsing backstory: '{playerBackstory}'");

        // 11.1 Character Gen Logic (simplified)
        // Normally LLMClient returns STR/DEX based on text.
        var liveState = new GameState_Live();
        liveState.PlayerHP = 150; // Guard HP
        liveState.PlayerGold = 0; // Broke
        liveState.Inventory.Add("sword_iron");
        liveState.Inventory.Add("rags_01");

        // 11.2 Macro-World Gen Logic
        var p = new LocationParams
        {
            Biome = "Лес",
            Type = "Деревня",
            Faction = "Нейтралы",
            PlayerLevel = 1,
            NarrativeContext = playerBackstory
        };

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(45));

        try
        {
            var generatedWorld = await _worldBuilder.GenerateLocation(p, cts.Token);

            // Push layout to Topology and SQLite
            ServiceLocator.Logger.LogInfo($"GameManager: World '{generatedWorld.LocationName}' established.");

            // Hand off liveState to the Simulation Thread
            // ServiceLocator.Simulation.InitializeWithState(liveState); // Assuming a method exists

            // Move to LocalMap view
            ServiceLocator.EventBus.EmitSignal(EventBus.SignalName.RequestStateChange, (int)GameState.Exploration);
        }
        catch (Exception ex)
        {
            ServiceLocator.Logger.LogError($"GameManager: Zero-State Protocol failed: {ex.Message}");
        }
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
            ServiceLocator.Logger.LogInfo("GameManager: Entered Exploration. Triggering Auto-Save mechanism.");
            ServiceLocator.Simulation.QueueCommand(new SaveGameCommand());
        }
    }
}
