using Xunit;
using DifferentWay.Core;
using System.Linq;

namespace DifferentWay.Tests
{
    public class CoreTests
    {
        [Fact]
        public void TestMVPInitialization()
        {
            var simulation = new Simulation();
            simulation.InitializeMVP();

            Assert.Single(simulation.Npcs);
            Assert.Equal("npc_innkeeper", simulation.Npcs[0].Id);
            Assert.Single(simulation.Locations);
            Assert.Equal("loc_tavern", simulation.Locations[0].Id);
            Assert.Single(simulation.Quests);
            Assert.Equal("quest_talk_to_innkeeper", simulation.Quests[0].Id);
        }

        [Fact]
        public void TestQuestCompletion()
        {
            var simulation = new Simulation();
            simulation.InitializeMVP();
            var questId = "quest_talk_to_innkeeper";

            simulation.CompleteQuest(questId);

            var quest = simulation.Quests.First(q => q.Id == questId);
            Assert.True(quest.IsCompleted);
        }
    }
}
