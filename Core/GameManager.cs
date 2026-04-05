using Godot;
using System;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }

    public Simulation Sim { get; private set; }
    public TimeManager TimeMgr { get; private set; }

    // Stage 2: Database and Saves
    public SQLiteConnector Database { get; private set; }
    public SaveManager Saves { get; private set; }

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }

        Instance = this;

        // Initialize core systems (The "Skeleton")
        TimeMgr = new TimeManager();
        AddChild(TimeMgr);

        Sim = new Simulation();
        AddChild(Sim);

        // Initialize Database and Save Systems
        Database = new SQLiteConnector();
        Saves = new SaveManager(Database);

        GD.Print("[Core] GameManager initialized. C# Skeleton running.");
    }
}
