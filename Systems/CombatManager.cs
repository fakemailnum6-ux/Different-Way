using System;
using System.Collections.Generic;
using System.Linq;
using DifferentWay.Systems.Models;

namespace DifferentWay.Systems;

public class CombatEntity
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public StatManager Stats { get; set; } = new StatManager();
    public CombatManager.CombatZone Zone { get; set; }
    public bool IsPlayer { get; set; }
    public int CurrentInitiative { get; set; }
}

public class CombatManager
{
    public enum CombatZone { Vanguard, Rearguard }

    private List<CombatEntity> _combatants = new();

    public void StartCombat(List<CombatEntity> participants)
    {
        _combatants = participants;
        CalculateTurnInitiative();
    }

    public void CalculateTurnInitiative()
    {
        // Initiative is based on DEX
        foreach (var entity in _combatants)
        {
            entity.CurrentInitiative = entity.Stats.DEX;
        }

        // Sort descending
        _combatants = _combatants.OrderByDescending(c => c.CurrentInitiative).ToList();
    }

    public bool CanAttackTarget(CombatEntity attacker, CombatEntity target, bool weaponIgnoresVanguard = false)
    {
        // Enemy melee cannot attack Rearguard until Vanguard is destroyed
        if (!attacker.IsPlayer && target.Zone == CombatZone.Rearguard && !weaponIgnoresVanguard)
        {
            bool playerVanguardAlive = _combatants.Any(c => c.IsPlayer && c.Zone == CombatZone.Vanguard && c.Stats.CurrentHP > 0);
            if (playerVanguardAlive)
            {
                return false;
            }
        }
        return true;
    }

    public bool AttemptFlee(CombatEntity player)
    {
        if (!player.IsPlayer) return false;

        // Mathematical check: Player DEX + Cardio vs Fastest Enemy DEX
        var fastestEnemy = _combatants.Where(c => !c.IsPlayer && c.Stats.CurrentHP > 0)
                                      .OrderByDescending(c => c.Stats.DEX)
                                      .FirstOrDefault();

        if (fastestEnemy == null) return true; // No enemies

        float playerFleeScore = player.Stats.DEX + player.Stats.Cardio;
        float enemyChaseScore = fastestEnemy.Stats.DEX;

        bool success = playerFleeScore > enemyChaseScore;

        if (!success)
        {
            // Fleeing failed: Skip turn and take backstab damage (handled by caller)
        }

        return success;
    }
}
