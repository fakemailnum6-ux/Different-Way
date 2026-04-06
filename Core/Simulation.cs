using System.Threading;
using Godot;
using DifferentWay.Systems;

namespace DifferentWay.Core;

public partial class GameState : RefCounted
{
    public StatManager PlayerStats { get; set; } = new StatManager();
    public QuestManager QuestManager { get; set; } = new QuestManager();
    public WorldTopology Topology { get; set; } = new WorldTopology();
    public InventoryManager PlayerInventory { get; set; } = new InventoryManager();

    public CraftingEngine CraftingEngine { get; set; } = new CraftingEngine();

    public StatManager GetPlayerStats() => PlayerStats;
    public QuestManager GetQuestManager() => QuestManager;
    public WorldTopology GetTopology() => Topology;
    public InventoryManager GetPlayerInventory() => PlayerInventory;

    // GDScript helper for CraftingUI
    public Godot.Collections.Dictionary GetRecipes()
    {
        var result = new Godot.Collections.Dictionary();
        foreach (var kvp in DataManager.Recipes)
        {
            var recipeNode = new Godot.Collections.Dictionary
            {
                { "id", kvp.Value.ResultItemId },
                { "name", kvp.Value.ResultItemId },
                { "difficulty", kvp.Value.BaseDifficulty }
            };

            var mats = new Godot.Collections.Array();
            foreach (var req in kvp.Value.RequiredMaterials)
            {
                var mat = new Godot.Collections.Dictionary
                {
                    { "id", req.Key },
                    { "name", req.Key },
                    { "amount", req.Value }
                };
                mats.Add(mat);
            }
            recipeNode["requirements"] = mats;

            result[kvp.Key] = recipeNode;
        }
        return result;
    }

    public void AttemptCraftingFromUI(string recipeId)
    {
        if (DataManager.Recipes.TryGetValue(recipeId, out var recipe))
        {
            CraftingEngine.AttemptCraft(recipe, PlayerStats.INT, PlayerInventory);
        }
        else
        {
            DifferentWay.Core.GameLogger.LogError($"Рецепт {recipeId} не найден!");
        }
    }
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
