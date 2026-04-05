using System.Collections.Generic;
using System.Linq;

namespace DifferentWay.Systems;

public class NpcNeeds
{
    public int Hunger { get; set; }
    public int Sleepiness { get; set; }
    public int Wealth { get; set; }
}

public class GOAPEngine
{
    // Evaluates NPC needs and calculates highest priority Goal
    public string CalculateRoutine(NpcNeeds needs)
    {
        // 4.6 Математически рассчитывает рутину NPC (Голод = 80 -> Цель: Поесть)
        Dictionary<string, int> goals = new Dictionary<string, int>
        {
            { "Поесть", needs.Hunger },
            { "Поспать", needs.Sleepiness },
            { "Заработать денег", 100 - needs.Wealth } // Desire to earn scales inversely with wealth
        };

        var highestPriorityGoal = goals.OrderByDescending(g => g.Value).FirstOrDefault();

        // If hunger is above 80, it overrides everything else
        if (needs.Hunger >= 80)
        {
            return "Поесть";
        }

        return highestPriorityGoal.Key;
    }
}
