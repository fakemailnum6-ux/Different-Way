using Godot;

namespace DifferentWay.Core
{
    public partial class GameManager : Node
    {
        public Simulation Simulation { get; private set; }
        public TimeManager TimeManager { get; private set; }

        public override void _Ready()
        {
            GD.Print("GameManager Initializing...");

            Simulation = new Simulation();
            AddChild(Simulation);

            TimeManager = new TimeManager();
            AddChild(TimeManager);

            GD.Print("GameManager Initialization Complete.");
        }
    }
}
