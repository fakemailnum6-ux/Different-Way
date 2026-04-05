using System.Text.Json.Serialization;

namespace DifferentWay.Systems.Models;

public class MobData
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("HP")] public int HP { get; set; }
    [JsonPropertyName("Damage")] public int Damage { get; set; }
    [JsonPropertyName("Armor")] public int Armor { get; set; }
    [JsonPropertyName("Dexterity")] public int? Dexterity { get; set; }
    [JsonPropertyName("Strength")] public int? Strength { get; set; }
    [JsonPropertyName("Tags")] public string[]? Tags { get; set; }
}

public class WeaponData
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("Damage")] public int Damage { get; set; }
    [JsonPropertyName("Durability")] public int Durability { get; set; }
    [JsonPropertyName("Weight")] public float Weight { get; set; }
    [JsonPropertyName("Price")] public int Price { get; set; }
    [JsonPropertyName("Description")] public string? Description { get; set; }
}

public class ArmorData
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("Armor")] public int Armor { get; set; }
    [JsonPropertyName("Durability")] public int Durability { get; set; }
    [JsonPropertyName("Weight")] public float Weight { get; set; }
    [JsonPropertyName("Price")] public int Price { get; set; }
    [JsonPropertyName("Description")] public string? Description { get; set; }
}

public class SkillData
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("Cost")] public int Cost { get; set; }
    [JsonPropertyName("CostType")] public string CostType { get; set; } = string.Empty;
    [JsonPropertyName("Description")] public string Description { get; set; } = string.Empty;
}

public class ConsumableData
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("Description")] public string Description { get; set; } = string.Empty;
}

public class MaterialData
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("Weight")] public float Weight { get; set; }
    [JsonPropertyName("Description")] public string Description { get; set; } = string.Empty;
}

public class StatusEffectData
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("Type")] public string Type { get; set; } = string.Empty;
    [JsonPropertyName("Duration")] public int Duration { get; set; }
    [JsonPropertyName("Description")] public string Description { get; set; } = string.Empty;
}
