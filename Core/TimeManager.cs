using Godot;

namespace DifferentWay.Core;

public partial class TimeManager : Node
{
    public double InGameTime { get; private set; }

    public override void _Process(double delta)
    {
        InGameTime += delta;
    }
}
