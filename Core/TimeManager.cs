using Godot;

namespace DifferentWay.Core
{
    public partial class TimeManager : Node
    {
        public float TimeOfDay { get; private set; } = 8.0f; // Start at 8 AM

        public override void _Process(double delta)
        {
            // Simulate time passing (e.g., 1 real second = 1 in-game minute)
            TimeOfDay += (float)delta * 0.016f;

            if (TimeOfDay >= 24.0f)
            {
                TimeOfDay -= 24.0f;
            }
        }
    }
}
