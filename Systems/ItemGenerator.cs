using Godot;
using System;
using System.Collections.Generic;
using DifferentWay.Core;

namespace DifferentWay.Systems
{
    public partial class ItemGenerator : Node
    {
        private EventBus _eventBus;
        private Random _rng = new Random();

        public override void _Ready()
        {
            _eventBus = GetNodeOrNull<EventBus>("/root/EventBus");
        }

        public void GenerateLoot(string mobId, DataManager data, Dictionary<string, int> playerInventory, CharacterStats playerStats)
        {
            if (data == null || !data.Mobs.ContainsKey(mobId)) return;

            var mob = data.Mobs[mobId];
            if (mob.Drops == null || mob.Drops.Count == 0) return;

            foreach (var dropId in mob.Drops)
            {
                // Basic drop chance: 30% base + Luck bonus
                int dropChance = 30 + playerStats.Luck * 2;
                if (_rng.Next(1, 101) <= dropChance)
                {
                    // Add item to inventory
                    if (playerInventory.ContainsKey(dropId)) playerInventory[dropId]++;
                    else playerInventory[dropId] = 1;

                    _eventBus?.EmitLogMessage("INFO", $"Выпал лут: {dropId}");
                }
            }
        }
    }
}
