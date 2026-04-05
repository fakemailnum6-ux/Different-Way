using System;
using Godot;

namespace DifferentWay.Tests.Simulation
{
    public class HeadlessRunner
    {
        public void RunSimulationTest()
        {
            var sim = new Core.Simulation();
            var time = new Core.TimeManager();

            GD.Print("Starting headless simulation test...");

            // Simulate 1000 ticks
            for (int i = 0; i < 1000; i++)
            {
                sim._Process(0.016);
                time._Process(0.016);
            }

            var finalState = sim.GetSnapshot();

            GD.Print($"Simulation test finished. Time of day: {time.TimeOfDay}, NPCs: {finalState.NpcCount}");
        }
    }
}
