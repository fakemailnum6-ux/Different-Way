using Godot;
using DifferentWay.Systems;

namespace DifferentWay.Core;

public partial class GameManager : Node
{
    public override void _Ready()
    {
        GD.Print("GameManager initialized.");
        DataManager.Initialize();
    }
}
