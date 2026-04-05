using System;
using System.Collections.Generic;
using System.Linq;

namespace DifferentWay.Core
{
    public class Simulation
    {
        public List<NPC> Npcs { get; private set; } = new List<NPC>();
        public List<Location> Locations { get; private set; } = new List<Location>();
        public List<Quest> Quests { get; private set; } = new List<Quest>();

        public void InitializeMVP()
        {
            var innkeeper = new NPC
            {
                Id = "npc_innkeeper",
                Name = "Innkeeper Barnaby",
                Description = "A stout man with a friendly face and a dirty apron."
            };
            Npcs.Add(innkeeper);

            var tavern = new Location
            {
                Id = "loc_tavern",
                Name = "The Sleepy Dragon Tavern",
                Description = "A cozy tavern with a roaring fireplace and the smell of roasted meat.",
                NpcIds = new List<string> { innkeeper.Id }
            };
            Locations.Add(tavern);

            var quest = new Quest
            {
                Id = "quest_talk_to_innkeeper",
                Title = "Welcome to the Tavern",
                Description = "Talk to Innkeeper Barnaby to get your bearings.",
                TargetNpcId = innkeeper.Id,
                IsCompleted = false
            };
            Quests.Add(quest);
        }

        public void CompleteQuest(string questId)
        {
            var quest = Quests.FirstOrDefault(q => q.Id == questId);
            if (quest != null)
            {
                quest.IsCompleted = true;
            }
        }
    }
}
