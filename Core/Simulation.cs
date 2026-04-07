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
    public EquipmentManager PlayerEquipment { get; set; } = new EquipmentManager();

    public CraftingEngine CraftingEngine { get; set; } = new CraftingEngine();
    public GOAPEngine GoapEngine { get; set; } = new GOAPEngine();
    public System.Collections.Generic.List<NpcState> ActiveNpcs { get; set; } = new();

    public DifferentWay.AI.LLMClient LlmClient { get; set; } = new DifferentWay.AI.LLMClient();
    public DifferentWay.AI.ContextManager Context { get; set; } = new DifferentWay.AI.ContextManager();
    public DifferentWay.AI.PromptBuilder PromptBuilder { get; set; }
    public DifferentWay.Database.MemoryManager? MemoryManager { get; set; }

    public GameState()
    {
        PromptBuilder = new DifferentWay.AI.PromptBuilder(Context);
    }

    public StatManager GetPlayerStats() => PlayerStats;
    public QuestManager GetQuestManager() => QuestManager;
    public WorldTopology GetTopology() => Topology;
    public InventoryManager GetPlayerInventory() => PlayerInventory;
    public EquipmentManager GetPlayerEquipment() => PlayerEquipment;

    private MerchantLogic _merchantLogic = new MerchantLogic();

    // GDScript helper for Merchant UI
    public int GetItemBuyPrice(string itemId)
    {
        int basePrice = _merchantLogic.GetItemBasePrice(itemId);
        return _merchantLogic.CalculateBuyPrice(basePrice, PlayerStats.Charisma);
    }

    public int GetItemSellPrice(string itemId)
    {
        int basePrice = _merchantLogic.GetItemBasePrice(itemId);
        return _merchantLogic.CalculateSellPrice(basePrice, PlayerStats.Charisma);
    }

    public bool AttemptBuyFromUI(string itemId, int price)
    {
        if (PlayerInventory.Gold >= price)
        {
            PlayerInventory.RemoveGold(price);
            PlayerInventory.AddItem(itemId, 1);
            GameLogger.Log($"Куплен предмет {itemId} за {price} золота.");
            return true;
        }
        GameLogger.LogError($"Недостаточно золота для покупки {itemId}.");
        return false;
    }

    public bool AttemptSellFromUI(string itemId, int price)
    {
        if (PlayerEquipment.EquippedWeapon?.Name == itemId || PlayerEquipment.EquippedArmor?.Name == itemId)
        {
            GameLogger.LogError($"Нельзя продать экипированный предмет {itemId}.");
            return false;
        }

        if (PlayerInventory.GetItemCount(itemId) > 0)
        {
            if (PlayerInventory.RemoveItem(itemId, 1))
            {
                PlayerInventory.AddGold(price);
                GameLogger.Log($"Продан предмет {itemId} за {price} золота.");
                return true;
            }
        }
        GameLogger.LogError($"Не удалось продать {itemId} - нет в инвентаре.");
        return false;
    }

    // GDScript helper for Equipping
    public void AttemptEquipFromUI(string itemId)
    {
        PlayerEquipment.EquipItem(itemId, PlayerInventory);
    }

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

    public bool AttemptTravel(string fromNodeId, string targetNodeId)
    {
        var route = Topology.GetRoute(fromNodeId, targetNodeId);
        if (route == null)
        {
            DifferentWay.Core.GameLogger.Log("Маршрута между этими точками не существует!");
            return false;
        }

        // Time cost: weight / speed (Assume speed is 10 for mapping)
        int minutesTaken = System.Math.Max(10, route.WeightDistance / 10);

        var tree = (SceneTree)Godot.Engine.GetMainLoop();
        var timeManager = tree?.Root.GetNodeOrNull<DifferentWay.Core.TimeManager>("/root/TimeManager");
        if (timeManager != null)
        {
            timeManager.AdvanceTime(minutesTaken);
            DifferentWay.Core.GameLogger.Log($"Путешествие заняло {minutesTaken} минут.");
        }

        // 4.4 Roll for Encounter
        bool hasEncounter = Topology.RollRandomEncounter(route, PlayerStats.Luck);
        if (hasEncounter)
        {
            DifferentWay.Core.GameLogger.Log("ВНИМАНИЕ! Случайная встреча на дороге!");
            var eventBus = tree?.Root.GetNodeOrNull<DifferentWay.Core.EventBus>("/root/EventBus");

            // Generate a random valid enemy ID for this vertical slice
            // Let's fallback to boar or bandit based on DangerLevel
            string enemyId = route.DangerLevel > 2 ? "bandit_rookie" : "boar_01";
            eventBus?.EmitSignal(DifferentWay.Core.EventBus.SignalName.EncounterTriggered, enemyId);

            // Note: True means we initiated the travel action successfully, even if ambushed.
            return true;
        }

        DifferentWay.Core.GameLogger.Log("Путешествие прошло спокойно.");
        return true;
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

    public void SaveApiKey(string key)
    {
        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
        {
#pragma warning disable CA1416 // Validate platform compatibility
            DifferentWay.Security.KeyManager.SaveKey(key);
#pragma warning restore CA1416
        }
        else
        {
            GameLogger.LogError("API Key encryption is only supported on Windows.");
        }
    }

    public void RespawnPlayer()
    {
        var stats = GameState_Live.PlayerStats;
        var inv = GameState_Live.PlayerInventory;

        // 4.2 Respawn & Penalties
        // Full heal
        stats.CurrentHP = stats.MaxHP;
        stats.CurrentStamina = stats.MaxStamina;
        stats.CurrentMana = stats.MaxMana;

        // Lose 20% of gold
        int lostGold = (int)(inv.Gold * 0.20f);
        inv.RemoveGold(lostGold);

        // Apply Trauma debuff (Broken bone)
        // 10.8: Сломанная кость: Перманентный дебафф (до доктора). -3 STR, -3 DEX.
        // We simulate a harsh debuff via StatusEffectManager here. (In a full system, you'd apply a permanent trait).
        GameLogger.Log($"Вы потеряли {lostGold} золота. Получена травма: Сломанная кость.");
        stats.STR = System.Math.Max(1, stats.STR - 3);
        stats.DEX = System.Math.Max(1, stats.DEX - 3);

        // TimeManager: Advance 1-3 days (Skipping complex time system implementation here, just logging it)
        GameLogger.Log("Прошло 2 дня. Вы очнулись в Таверне.");

        // Emit Global Event
        var tree = (SceneTree)Godot.Engine.GetMainLoop();
        var eventBus = tree?.Root.GetNodeOrNull<DifferentWay.Core.EventBus>("/root/EventBus");
        eventBus?.EmitSignal(DifferentWay.Core.EventBus.SignalName.PlayerDied);
    }

    // Helper for GDScript to instantiate Combat entities easily
    public void StartEncounterCustom(DifferentWay.Systems.CombatManager combatManager, string enemyId)
    {
        var entities = new System.Collections.Generic.List<DifferentWay.Systems.CombatEntity>();

        entities.Add(new DifferentWay.Systems.CombatEntity
        {
            Id = "player_1",
            Name = "Герой",
            Stats = GameState_Live.PlayerStats,
            IsPlayer = true,
            Zone = DifferentWay.Systems.CombatManager.CombatZone.Vanguard,
            WeaponDamage = GameState_Live.PlayerEquipment.GetTotalWeaponDamage(),
            ArmorValue = GameState_Live.PlayerEquipment.GetTotalArmorValue()
        });

        // Simple enemy fetch logic from DataManager
        var enemyStats = new DifferentWay.Systems.StatManager();
        string name = "Враг";

        // Mobs are loaded in DataManager if we had them full, here we mock basic fetch based on bestiary JSON memory
        if (enemyId == "bandit_rookie")
        {
            name = "Бандит-новичок";
            enemyStats.CurrentHP = 40;
            enemyStats.DEX = 12;
        }
        else if (enemyId == "boar_01")
        {
            name = "Дикий Кабан";
            enemyStats.CurrentHP = 50;
            enemyStats.DEX = 14;
        }
        else
        {
            name = "Неизвестный Монстр";
            enemyStats.CurrentHP = 20;
        }

        entities.Add(new DifferentWay.Systems.CombatEntity
        {
            Id = enemyId,
            Name = name,
            Stats = enemyStats,
            IsPlayer = false,
            Zone = DifferentWay.Systems.CombatManager.CombatZone.Vanguard,
            WeaponDamage = 5,
            ArmorValue = 1
        });

        combatManager.StartCombat(entities);
    }

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
            WeaponDamage = GameState_Live.PlayerEquipment.GetTotalWeaponDamage(),
            ArmorValue = GameState_Live.PlayerEquipment.GetTotalArmorValue()
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
