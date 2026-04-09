using Godot;
using System.Collections.Generic;
using DifferentWay.Core;

namespace DifferentWay.Systems
{
    public class CraftingRecipe
    {
        public string ResultItemId { get; set; }
        public Dictionary<string, int> RequiredMaterials { get; set; } = new Dictionary<string, int>();
        public int RequiredInt { get; set; } = 5;
    }

    public partial class CraftingEngine : Node
    {
        private EventBus _eventBus;

        // Stub recipes. In a full implementation, these would load from JSON.
        private List<CraftingRecipe> _recipes = new List<CraftingRecipe>
        {
            new CraftingRecipe
            {
                ResultItemId = "potion_hp_small",
                RequiredMaterials = { {"mat_herb_healing", 2} },
                RequiredInt = 8
            },
            new CraftingRecipe
            {
                ResultItemId = "club_wood",
                RequiredMaterials = { {"mat_log_wood", 1}, {"mat_leather_straps", 1} },
                RequiredInt = 5
            }
        };

        public override void _Ready()
        {
            _eventBus = GetNodeOrNull<EventBus>("/root/EventBus");
        }

        public List<CraftingRecipe> GetRecipes() => _recipes;

        public bool CanCraft(Dictionary<string, int> inventory, CraftingRecipe recipe)
        {
            foreach (var req in recipe.RequiredMaterials)
            {
                if (!inventory.ContainsKey(req.Key) || inventory[req.Key] < req.Value)
                {
                    return false;
                }
            }
            return true;
        }

        public bool TryCraft(CharacterStats crafterStats, Dictionary<string, int> inventory, CraftingRecipe recipe)
        {
            if (!CanCraft(inventory, recipe))
            {
                _eventBus?.EmitLogMessage("WARNING", "Не хватает материалов для крафта.");
                return false;
            }

            // Math check (Int based success chance)
            // Simple formula: base 50% + (INT - ReqINT) * 5%
            int chance = 50 + (crafterStats.INT - recipe.RequiredInt) * 5;
            chance = Mathf.Clamp(chance, 10, 100);

            var rng = new System.Random();
            bool success = rng.Next(1, 101) <= chance;

            // Consume materials
            foreach (var req in recipe.RequiredMaterials)
            {
                inventory[req.Key] -= req.Value;
                if (inventory[req.Key] <= 0)
                {
                    inventory.Remove(req.Key);
                }
            }

            if (success)
            {
                if (inventory.ContainsKey(recipe.ResultItemId)) inventory[recipe.ResultItemId]++;
                else inventory[recipe.ResultItemId] = 1;

                _eventBus?.EmitLogMessage("INFO", $"Успешный крафт: {recipe.ResultItemId}");
                return true;
            }
            else
            {
                _eventBus?.EmitLogMessage("WARNING", "Крафт провалился! Материалы потеряны.");
                return false;
            }
        }
    }
}
