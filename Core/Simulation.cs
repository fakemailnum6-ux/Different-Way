using System.Threading;
using Godot;
using DifferentWay.Systems;

namespace DifferentWay.Core;

public partial class GameState : RefCounted
{
    public StatManager PlayerStats { get; set; } = new StatManager();
    public QuestManager QuestManager { get; set; } = new QuestManager();

    public StatManager GetPlayerStats() => PlayerStats;
    public QuestManager GetQuestManager() => QuestManager;
}

public partial class Simulation : Node
{
    private ReaderWriterLockSlim _stateLock = new ReaderWriterLockSlim();

    public GameState GameState_Live { get; private set; } = new GameState();
    public GameState GameState_Snapshot { get; private set; } = new GameState();

    public void UpdateSnapshot()
    {
        _stateLock.EnterWriteLock();
        try
        {
            // Shallow copy or deep copy of state for UI to read from
            GameState_Snapshot = GameState_Live;
        }
        finally
        {
            _stateLock.ExitWriteLock();
        }
    }

    public GameState GetLiveState()
    {
        return GameState_Live;
    }

    // Helper for GDScript to instantiate Combat entities easily
    public void StartEncounter(DifferentWay.Systems.CombatManager combatManager)
    {
        var entities = new System.Collections.Generic.List<DifferentWay.Systems.CombatEntity>();

        // Add Player
        entities.Add(new DifferentWay.Systems.CombatEntity
        {
            Id = "player_1",
            Name = "Герой",
            Stats = GameState_Live.PlayerStats,
            IsPlayer = true,
            Zone = DifferentWay.Systems.CombatManager.CombatZone.Vanguard,
            WeaponDamage = 8,
            ArmorValue = 2
        });

        // Add a Wolf Enemy
        var wolfStats = new DifferentWay.Systems.StatManager();
        wolfStats.STR = 12;
        wolfStats.DEX = 14;
        wolfStats.CurrentHP = 35;
        wolfStats.END = 8;

        entities.Add(new DifferentWay.Systems.CombatEntity
        {
            Id = "enemy_wolf_1",
            Name = "Лесной Волк",
            Stats = wolfStats,
            IsPlayer = false,
            Zone = DifferentWay.Systems.CombatManager.CombatZone.Vanguard,
            WeaponDamage = 8,
            ArmorValue = 2
        });

        combatManager.StartCombat(entities);
    }
}
