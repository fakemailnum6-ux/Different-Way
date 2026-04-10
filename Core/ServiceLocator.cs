using Godot;
using System;

public static class ServiceLocator
{
    public static GameManager GameManager { get; private set; }
    public static Simulation Simulation { get; private set; }
    public static EventBus EventBus { get; private set; }
    public static Logger Logger { get; private set; }
    public static DataManager DataManager { get; private set; }
    public static LocalizationManager LocalizationManager { get; private set; }
    public static SaveManager SaveManager { get; private set; }

    public static void Initialize(SceneTree tree)
    {
        GameManager = tree.Root.GetNodeOrNull<GameManager>("/root/GameManager");
        Simulation = tree.Root.GetNodeOrNull<Simulation>("/root/Simulation");
        EventBus = tree.Root.GetNodeOrNull<EventBus>("/root/EventBus");
        Logger = tree.Root.GetNodeOrNull<Logger>("/root/Logger");

        DataManager = new DataManager();
        LocalizationManager = new LocalizationManager();
        SaveManager = new SaveManager();

        if (GameManager == null) GD.PrintErr("ServiceLocator: GameManager not found.");
        if (Simulation == null) GD.PrintErr("ServiceLocator: Simulation not found.");
        if (EventBus == null) GD.PrintErr("ServiceLocator: EventBus not found.");
        if (Logger == null) GD.PrintErr("ServiceLocator: Logger not found.");
    }
}
