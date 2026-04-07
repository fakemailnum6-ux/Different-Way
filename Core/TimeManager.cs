using Godot;

namespace DifferentWay.Core;

using System;

public partial class TimeManager : Node
{
    // Real time seconds to in-game minutes
    // E.g., 1 real second = 1 in-game minute
    private const float TimeScale = 1.0f;

    public int CurrentDay { get; private set; } = 1;
    public int CurrentHour { get; private set; } = 8; // Starts at 8:00 AM
    public int CurrentMinute { get; private set; } = 0;

    private float _minuteAccumulator = 0f;
    private float _regenerationAccumulator = 0f;

    public bool IsPaused { get; set; } = false;

    public override void _Process(double delta)
    {
        if (IsPaused) return;

        float dt = (float)delta;

        // Time advancement logic
        _minuteAccumulator += dt * TimeScale;
        if (_minuteAccumulator >= 1.0f) // 1 in-game minute passed
        {
            _minuteAccumulator -= 1.0f;
            AdvanceTime(1);
        }

        // Regeneration logic (ticks every 5 real seconds = 5 in-game minutes roughly)
        _regenerationAccumulator += dt;
        if (_regenerationAccumulator >= 5.0f)
        {
            _regenerationAccumulator = 0f;
            ProcessRegeneration();
        }
    }

    public void AdvanceTime(int minutes)
    {
        CurrentMinute += minutes;

        bool hourChanged = false;
        while (CurrentMinute >= 60)
        {
            CurrentMinute -= 60;
            CurrentHour++;
            hourChanged = true;
        }

        while (CurrentHour >= 24)
        {
            CurrentHour -= 24;
            CurrentDay++;
            GameLogger.Log($"Наступил новый день! (День {CurrentDay})");
        }

        // Tick GOAP Engine for all NPCs
        var tree = (SceneTree)Godot.Engine.GetMainLoop();
        var simulation = tree?.Root.GetNodeOrNull<DifferentWay.Core.Simulation>("/root/Simulation");
        if (simulation != null && hourChanged) // Update GOAP once per in-game hour
        {
            foreach (var npc in simulation.GameState_Live.ActiveNpcs)
            {
                simulation.GameState_Live.GoapEngine.UpdateNpcRoutine(npc, CurrentHour);
            }
        }

        // Notify systems
        var eventBus = tree?.Root.GetNodeOrNull<DifferentWay.Core.EventBus>("/root/EventBus");
        eventBus?.EmitSignal(DifferentWay.Core.EventBus.SignalName.TimeAdvanced, CurrentDay, CurrentHour, CurrentMinute);
    }

    public void AdvanceDays(int days)
    {
        CurrentDay += days;
        GameLogger.Log($"Время пролетело... Наступил День {CurrentDay}.");

        var tree = (SceneTree)Godot.Engine.GetMainLoop();
        var eventBus = tree?.Root.GetNodeOrNull<DifferentWay.Core.EventBus>("/root/EventBus");
        eventBus?.EmitSignal(DifferentWay.Core.EventBus.SignalName.TimeAdvanced, CurrentDay, CurrentHour, CurrentMinute);
    }

    private void ProcessRegeneration()
    {
        var tree = (SceneTree)Godot.Engine.GetMainLoop();
        var simulation = tree?.Root.GetNodeOrNull<DifferentWay.Core.Simulation>("/root/Simulation");
        if (simulation == null) return;

        var playerStats = simulation.GameState_Live.PlayerStats;

        // Base regeneration mathematically derived from Arc.md
        // Health: Slow passive regen based on END
        if (playerStats.CurrentHP < playerStats.MaxHP && playerStats.CurrentHP > 0)
        {
            int hpRegen = Math.Max(1, playerStats.END / 2);
            playerStats.CurrentHP = Math.Min(playerStats.MaxHP, playerStats.CurrentHP + hpRegen);
        }

        // Stamina: Regenerates based on Cardio
        if (playerStats.CurrentStamina < playerStats.MaxStamina)
        {
            int stamRegen = Math.Max(1, (int)playerStats.Cardio);
            playerStats.CurrentStamina = Math.Min(playerStats.MaxStamina, playerStats.CurrentStamina + stamRegen);
        }

        // Mana: Regenerates based on INT
        if (playerStats.CurrentMana < playerStats.MaxMana)
        {
            int manaRegen = Math.Max(1, playerStats.INT / 3);
            playerStats.CurrentMana = Math.Min(playerStats.MaxMana, playerStats.CurrentMana + manaRegen);
        }
    }

    // GDScript Formatter
    public string GetFormattedTime()
    {
        return $"День {CurrentDay}, {CurrentHour:D2}:{CurrentMinute:D2}";
    }
}
