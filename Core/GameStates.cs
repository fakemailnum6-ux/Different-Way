using System.Collections.Generic;
using System.Collections.Immutable;

// Mutable core simulation state
public class GameState_Live
{
    public long StateVersion { get; set; } = 0;

    // Example states based on Arc.md 3.1
    public int PlayerHP { get; set; } = 100;
    public int PlayerGold { get; set; } = 0;
    public List<string> Inventory { get; set; } = new List<string>();

    // Creates an immutable deep-copy snapshot
    public GameState_Snapshot CreateSnapshot()
    {
        return new GameState_Snapshot(
            StateVersion,
            PlayerHP,
            PlayerGold,
            Inventory.ToImmutableList()
        );
    }
}

// Immutable snapshot for UI
public record GameState_Snapshot(
    long Version,
    int PlayerHP,
    int PlayerGold,
    IReadOnlyList<string> Inventory
);
