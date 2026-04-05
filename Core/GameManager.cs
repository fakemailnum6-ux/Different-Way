using Godot;
using System;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }

    public Simulation Sim { get; private set; }
    public TimeManager TimeMgr { get; private set; }

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

        GD.Print("[Core] GameManager initialized. C# Skeleton running.");
    }
}
