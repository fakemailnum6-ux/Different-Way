using Godot;
using System;
using System.Collections.Generic;

namespace DifferentWay.Systems
{
    // A simplified mathematical Goal-Oriented Action Planning model
    public class NpcState
    {
        public int Hunger { get; set; } = 0;   // 0 to 100
        public int Energy { get; set; } = 100; // 0 to 100
        public int Wealth { get; set; } = 10;
        public bool IsWorking { get; set; } = false;

        public string CurrentGoalStr { get; set; } = "Idling";
    }

    public partial class GOAPEngine : Node
    {
        public override void _Ready()
        {
        }

        // Called during background simulation ticks for NPCs in loaded regions
        public void UpdateNpcRoutine(NpcState state, int timeDeltaMinutes)
        {
            // Passive drain
            state.Hunger += timeDeltaMinutes / 10; // +6 hunger per hour
            state.Energy -= timeDeltaMinutes / 15; // -4 energy per hour

            // Cap values
            if (state.Hunger > 100) state.Hunger = 100;
            if (state.Energy < 0) state.Energy = 0;

            // Evaluate Priorities (Math over meat)
            if (state.Energy < 20)
            {
                state.CurrentGoalStr = "Спит. Сильно истощен.";
                state.IsWorking = false;
            }
            else if (state.Hunger > 80)
            {
                state.CurrentGoalStr = "Ищет еду. Очень голоден.";
                state.IsWorking = false;

                // Simulate simple action resolution
                if (state.Wealth >= 5)
                {
                    state.Wealth -= 5;
                    state.Hunger -= 50; // Bought food
                    state.CurrentGoalStr = "Ест купленную еду.";
                }
            }
            else if (state.Energy > 50 && state.Hunger < 50)
            {
                state.CurrentGoalStr = "Работает/Патрулирует.";
                state.IsWorking = true;

                // Simulate earning money
                if (timeDeltaMinutes > 60)
                {
                    state.Wealth += 2;
                }
            }
            else
            {
                state.CurrentGoalStr = "Отдыхает.";
                state.IsWorking = false;
            }
        }
    }
}
