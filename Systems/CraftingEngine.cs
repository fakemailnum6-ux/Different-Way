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
    public bool ValidateIngredients(CraftingRecipe recipe, InventoryManager inventory)
    {
        return inventory.HasItems(recipe.RequiredMaterials);
    }

    public int CalculateSuccessChance(int difficulty, int playerIntelligence)
    {
        // Base math: INT * 5 vs Difficulty
        return Math.Clamp((playerIntelligence * 5) - difficulty + 50, 5, 95);
    }

    public bool AttemptCraft(CraftingRecipe recipe, int playerIntelligence, InventoryManager inventory)
    {
        if (!ValidateIngredients(recipe, inventory))
        {
            DifferentWay.Core.GameLogger.Log("Недостаточно материалов для крафта.");
            return false;
        }

        int successChance = CalculateSuccessChance(recipe.BaseDifficulty, playerIntelligence);
        int roll = _rnd.Next(1, 101);

        bool isSuccess = roll <= successChance;

        // Consume ingredients regardless of success or failure
        foreach (var material in recipe.RequiredMaterials)
        {
            inventory.RemoveItem(material.Key, material.Value);
        }

        if (isSuccess)
        {
            DifferentWay.Core.GameLogger.Log($"Крафт успешен! Получен предмет: {recipe.ResultItemId}");
            inventory.AddItem(recipe.ResultItemId, 1);
            // Optionally, signal AI for dynamic naming here
        }
        else
        {
            DifferentWay.Core.GameLogger.Log($"Крафт провален. Материалы утеряны.");
        }

        return isSuccess;
    }
}
