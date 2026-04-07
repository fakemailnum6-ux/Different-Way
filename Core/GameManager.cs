using Godot;
using DifferentWay.Systems;
using DifferentWay.Database;

namespace DifferentWay.Core;

public partial class GameManager : Node
{
    public override void _Ready()
    {
        GameLogger.Log("GameManager initializing...");

        var saveManager = new SaveManager();
        saveManager.SetupDirectories();
        GameLogger.Log("User directories configured.");

        saveManager.LoadGame("autosave");
        GameLogger.Log("Database 'autosave' initialized.");

        var tree = (SceneTree)Godot.Engine.GetMainLoop();
        var simulation = tree?.Root.GetNodeOrNull<DifferentWay.Core.Simulation>("/root/Simulation");
        if (simulation != null)
        {
            var loadedTopology = saveManager.LoadWorldTopology();
            if (loadedTopology != null)
            {
                simulation.GameState_Live.Topology = loadedTopology;
                GameLogger.Log("World topology loaded from disk.");
            }
            else
            {
                GameLogger.Log("No world topology found. Generating new macro world...");
                simulation.GameState_Live.Topology.GenerateMacroWorld();
                saveManager.SaveWorldTopology(simulation.GameState_Live.Topology);
            }
        }

        DataManager.Initialize();

        // Initialize API Key if present
        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
        {
#pragma warning disable CA1416 // Validate platform compatibility
            string savedKey = DifferentWay.Security.KeyManager.LoadKey();
            if (!string.IsNullOrEmpty(savedKey) && simulation != null)
            {
                simulation.GameState_Live.LlmClient.SetCredentials(savedKey, "https://api.openai.com/v1/chat/completions");
                GameLogger.Log("OpenAI API Key loaded and injected into LLMClient.");
            }
#pragma warning restore CA1416
        }
        else
        {
            GameLogger.Log("API Key encryption only supported on Windows. Using MOCK fallback.");
        }

        GameLogger.Log("GameManager initialized.");
    }
}
