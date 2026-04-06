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

        DataManager.Initialize();

        GameLogger.Log("GameManager initialized.");
    }
}
