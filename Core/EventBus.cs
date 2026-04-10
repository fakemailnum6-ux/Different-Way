using Godot;
using System;

public partial class EventBus : Node
{
    [Signal]
    public delegate void RequestStateChangeEventHandler(int stateEnum);

    [Signal]
    public delegate void OnHealthChangedEventHandler();

    [Signal]
    public delegate void PlayerDiedEventHandler();

    [Signal]
    public delegate void MobKilledEventHandler(string mobId);

    [Signal]
    public delegate void StateTransitionedEventHandler(int newStateEnum);

    [Signal]
    public delegate void LanguageChangedEventHandler(string langCode);

    // Provide a generic GDScript interop method if needed
    public void Emit(string signalName, params Variant[] args)
    {
        EmitSignal(signalName, args);
    }
}
