using System;
using System.Collections.Generic;
using System.Linq;
using DifferentWay.Systems.Models;
using Godot;

namespace DifferentWay.Systems;

public class CombatEntity
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public StatManager Stats { get; set; } = new StatManager();
    public CombatManager.CombatZone Zone { get; set; }
    public bool IsPlayer { get; set; }
    public int CurrentInitiative { get; set; }
    public int WeaponDamage { get; set; } = 5; // Default fist damage if unarmed
    public int ArmorValue { get; set; } = 0;
}

public partial class CombatManager : RefCounted
{
    public enum CombatZone { Vanguard, Rearguard }
    public enum CombatState { NotStarted, PlayerTurn, EnemyTurn, CombatEnded }

    private List<CombatEntity> _combatants = new();
    private CombatState _currentState = CombatState.NotStarted;

    // UI Event Signals for Godot
    [Signal] public delegate void CombatLogUpdatedEventHandler(string logMessage);
    [Signal] public delegate void TurnChangedEventHandler(bool isPlayerTurn, string activeEntityName);
    [Signal] public delegate void CombatEndedEventHandler(bool playerWon);
    [Signal] public delegate void EntityStatusChangedEventHandler();

    private CombatEntity? _playerEntity;
    private int _currentTurnIndex = 0;

    public void StartCombat(List<CombatEntity> participants)
    {
        _combatants = participants;
        _playerEntity = _combatants.FirstOrDefault(c => c.IsPlayer);

        EmitLog("--- Бой начался ---");
        CalculateTurnInitiative();

        _currentState = CombatState.PlayerTurn;
        _currentTurnIndex = -1;
        AdvanceTurn();
    }

    public void CalculateTurnInitiative()
    {
        foreach (var entity in _combatants)
        {
            entity.CurrentInitiative = entity.Stats.DEX;
        }
        // Sort descending by initiative
        _combatants = _combatants.OrderByDescending(c => c.CurrentInitiative).ToList();

        EmitLog("Инициатива рассчитана.");
    }

    private void AdvanceTurn()
    {
        if (CheckCombatEnd()) return;

        _currentTurnIndex++;
        if (_currentTurnIndex >= _combatants.Count)
        {
            _currentTurnIndex = 0; // New Round
            EmitLog("--- Новый раунд ---");
        }

        var activeEntity = _combatants[_currentTurnIndex];

        // Skip dead entities
        if (activeEntity.Stats.CurrentHP <= 0)
        {
            AdvanceTurn();
            return;
        }

        _currentState = activeEntity.IsPlayer ? CombatState.PlayerTurn : CombatState.EnemyTurn;

        EmitSignal(SignalName.TurnChanged, activeEntity.IsPlayer, activeEntity.Name);

        if (!activeEntity.IsPlayer)
        {
            // Simple Enemy AI: Attack Player
            ExecuteEnemyTurn(activeEntity);
        }
    }

    // Called by UI when player clicks 'Attack'
    public void PlayerAttackTarget(string targetId)
    {
        if (_currentState != CombatState.PlayerTurn || _playerEntity == null) return;

        var target = _combatants.FirstOrDefault(c => c.Id == targetId && c.Stats.CurrentHP > 0);
        if (target == null)
        {
            EmitLog("Неверная цель!");
            return;
        }

        if (!CanAttackTarget(_playerEntity, target))
        {
            EmitLog($"Нельзя атаковать цель {target.Name} в Арьергарде, пока жив Авангард!");
            return;
        }

        PerformAttack(_playerEntity, target);

        EmitSignal(SignalName.EntityStatusChanged);
        AdvanceTurn();
    }

    // Called by UI if player waits or timer runs out
    public void PlayerDefend()
    {
        if (_currentState != CombatState.PlayerTurn || _playerEntity == null) return;

        EmitLog($"{_playerEntity.Name} уходит в глухую оборону (пропуск хода).");
        AdvanceTurn();
    }

    private void ExecuteEnemyTurn(CombatEntity enemy)
    {
        if (_playerEntity == null || _playerEntity.Stats.CurrentHP <= 0) return;

        EmitLog($"Ход противника: {enemy.Name} атакует...");
        PerformAttack(enemy, _playerEntity);

        EmitSignal(SignalName.EntityStatusChanged);
        AdvanceTurn();
    }

    private void PerformAttack(CombatEntity attacker, CombatEntity target)
    {
        var result = DamageCalculator.CalculateMeleeAttack(
            attacker.Stats.STR,
            attacker.Stats.DEX,
            attacker.Stats.Luck,
            attacker.WeaponDamage,
            target.ArmorValue,
            target.Stats.Evasion
        );

        if (result.ResultType == DamageCalculator.HitResult.Miss || result.ResultType == DamageCalculator.HitResult.CriticalMiss)
        {
            EmitLog($"{attacker.Name} ПРОМАХНУЛСЯ по {target.Name}!");
        }
        else
        {
            string critStr = result.ResultType == DamageCalculator.HitResult.CriticalHit ? "КРИТИЧЕСКИЙ УДАР! " : "";
            target.Stats.ApplyDamage(result.FinalDamage);
            EmitLog($"{critStr}{attacker.Name} наносит {result.FinalDamage} урона {target.Name}. (Осталось HP: {target.Stats.CurrentHP})");

            if (target.Stats.CurrentHP <= 0)
            {
                EmitLog($"*** {target.Name} ПОГИБАЕТ! ***");
            }
        }
    }

    public bool CanAttackTarget(CombatEntity attacker, CombatEntity target, bool weaponIgnoresVanguard = false)
    {
        if (!attacker.IsPlayer && target.Zone == CombatZone.Rearguard && !weaponIgnoresVanguard)
        {
            bool playerVanguardAlive = _combatants.Any(c => c.IsPlayer && c.Zone == CombatZone.Vanguard && c.Stats.CurrentHP > 0);
            return !playerVanguardAlive;
        }

        if (attacker.IsPlayer && target.Zone == CombatZone.Rearguard && !weaponIgnoresVanguard)
        {
             bool enemyVanguardAlive = _combatants.Any(c => !c.IsPlayer && c.Zone == CombatZone.Vanguard && c.Stats.CurrentHP > 0);
             return !enemyVanguardAlive;
        }

        return true;
    }

    public void AttemptFlee()
    {
        if (_currentState != CombatState.PlayerTurn || _playerEntity == null) return;

        var fastestEnemy = _combatants.Where(c => !c.IsPlayer && c.Stats.CurrentHP > 0)
                                      .OrderByDescending(c => c.Stats.DEX)
                                      .FirstOrDefault();

        if (fastestEnemy == null)
        {
            EmitLog("Врагов нет, побег успешен.");
            EndCombat(true);
            return;
        }

        float playerFleeScore = _playerEntity.Stats.DEX + _playerEntity.Stats.Cardio;
        float enemyChaseScore = fastestEnemy.Stats.DEX;

        bool success = playerFleeScore > enemyChaseScore;

        if (success)
        {
            EmitLog("Вы успешно сбежали с поля боя!");
            EndCombat(false); // Flee is not a "win"
        }
        else
        {
            EmitLog("Побег не удался! Враг настигает вас.");
            // Take automatic backstab damage on fail
            PerformAttack(fastestEnemy, _playerEntity);
            EmitSignal(SignalName.EntityStatusChanged);
            AdvanceTurn();
        }
    }

    private bool CheckCombatEnd()
    {
        if (_playerEntity == null || _playerEntity.Stats.CurrentHP <= 0)
        {
            EmitLog("Вы погибли в бою...");
            EndCombat(false);
            return true;
        }

        bool enemiesAlive = _combatants.Any(c => !c.IsPlayer && c.Stats.CurrentHP > 0);
        if (!enemiesAlive)
        {
            EmitLog("Все враги повержены! Вы победили!");
            EndCombat(true);
            return true;
        }

        return false;
    }

    private void EndCombat(bool playerWon)
    {
        _currentState = CombatState.CombatEnded;
        EmitSignal(SignalName.CombatEnded, playerWon);
    }

    private void EmitLog(string message)
    {
        DifferentWay.Core.GameLogger.Log("[Бой] " + message);
        EmitSignal(SignalName.CombatLogUpdated, message);
    }

    // GDScript Getters
    public Godot.Collections.Array<Godot.Collections.Dictionary> GetEnemyList()
    {
        var list = new Godot.Collections.Array<Godot.Collections.Dictionary>();
        foreach (var c in _combatants.Where(e => !e.IsPlayer))
        {
            var dict = new Godot.Collections.Dictionary
            {
                { "id", c.Id },
                { "name", c.Name },
                { "hp", c.Stats.CurrentHP },
                { "max_hp", c.Stats.MaxHP },
                { "zone", c.Zone.ToString() }
            };
            list.Add(dict);
        }
        return list;
    }
}
