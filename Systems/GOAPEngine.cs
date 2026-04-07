using System.Collections.Generic;
using System.Linq;

namespace DifferentWay.Systems;

public class NpcNeeds
{
    public int Hunger { get; set; }
    public int Sleepiness { get; set; }
    public int Wealth { get; set; }
}

public class NpcState
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Profession { get; set; } = string.Empty;
    public NpcNeeds Needs { get; set; } = new NpcNeeds();
    public string CurrentLocationId { get; set; } = string.Empty;
    public string CurrentGoal { get; set; } = "Отдыхать";
}

public class GOAPEngine
{
    // Evaluates NPC needs and calculates highest priority Goal based on time
    public void UpdateNpcRoutine(NpcState npc, int currentHour)
    {
        // Needs increment over time
        npc.Needs.Hunger += 5;
        npc.Needs.Sleepiness += 3;

        // Schedule-based overrides
        if (currentHour >= 22 || currentHour < 6)
        {
            npc.Needs.Sleepiness += 10;
        }
        else if (currentHour >= 8 && currentHour <= 18)
        {
            // Work hours
            npc.CurrentGoal = "Работать";
            npc.CurrentLocationId = npc.Profession == "Кузнец" ? "loc_forge" : "loc_square";
        }

        // Needs-based evaluation overrides schedule
        if (npc.Needs.Sleepiness >= 80)
        {
            npc.CurrentGoal = "Поспать";
            npc.CurrentLocationId = "loc_home";
            npc.Needs.Sleepiness = 0; // Simulate sleeping
        }
        else if (npc.Needs.Hunger >= 70)
        {
            npc.CurrentGoal = "Поесть";
            npc.CurrentLocationId = "loc_tavern";
            npc.Needs.Hunger = 0; // Simulate eating
            npc.Needs.Wealth -= 5;
        }

        DifferentWay.Core.GameLogger.Log($"NPC {npc.Name} (GOAP): Цель - {npc.CurrentGoal}, Локация - {npc.CurrentLocationId}");
    }
}
