using Godot;

namespace DifferentWay.Core;

public partial class EventBus : Node
{
    // Basic signal definitions for central event bus
    [Signal]
    public delegate void PlayerDiedEventHandler();

    [Signal]
    public delegate void ItemCraftedEventHandler(string itemId);

    [Signal]
    public delegate void QuestAcceptedEventHandler(string questName);

    [Signal]
    public delegate void CombatStartedEventHandler();

    [Signal]
    public delegate void CombatEndedEventHandler(bool playerWon);
}
