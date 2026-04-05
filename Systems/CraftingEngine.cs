using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace DifferentWay.Systems;

public class CraftingRecipe
{
    public string ResultItemId { get; set; } = string.Empty;
    public Dictionary<string, int> RequiredMaterials { get; set; } = new();
    public int BaseDifficulty { get; set; }
}

public partial class CraftingEngine : RefCounted
{
    private readonly Random _rnd = new Random();

    // 4.6 CraftingEngine: Валидирует наличие ингредиентов. Рассчитывает шанс успеха на основе Интеллекта.
    public bool ValidateIngredients(CraftingRecipe recipe, Dictionary<string, int> playerInventory)
    {
        foreach (var material in recipe.RequiredMaterials)
        {
            if (!playerInventory.ContainsKey(material.Key) || playerInventory[material.Key] < material.Value)
            {
                return false;
            }
        }
        return true;
    }

    public int CalculateSuccessChance(int difficulty, int playerIntelligence)
    {
        // Base math: INT * 5 vs Difficulty
        return Math.Clamp((playerIntelligence * 5) - difficulty + 50, 5, 95);
    }

    public bool AttemptCraft(CraftingRecipe recipe, int playerIntelligence)
    {
        int successChance = CalculateSuccessChance(recipe.BaseDifficulty, playerIntelligence);
        int roll = _rnd.Next(1, 101);

        bool isSuccess = roll <= successChance;

        // If success, AI is queried for dynamic name/description (handled outside engine)
        return isSuccess;
    }
}
