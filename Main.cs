using Godot;
using DifferentWay.Systems;
using DifferentWay.UI;

namespace DifferentWay
{
    public partial class Main : Node
    {
        private WorldBuilder _worldBuilder;
        private LocalMap _localMap;

        public override async void _Ready()
        {
            _worldBuilder = GetNode<WorldBuilder>("/root/WorldBuilder");
            _localMap = GetNode<LocalMap>("LocalMap");

            // Execute Zero-State World Generation
            var p = new LocationParams
            {
                Biome = "Forest",
                SettlementType = "Village",
                Faction = "Neutral",
                PlayerLevel = 1,
                NarrativeContext = "Бывший стражник ищет пристанище."
            };

            var settlement = await _worldBuilder.GenerateLocationAsync(p);

            // Pass to UI to render
            _localMap.LoadSettlement(settlement);

            GD.Print("Phase 1/4 Zero-State generation complete. Settlement rendered on LocalMap.");
        }
    }
}
