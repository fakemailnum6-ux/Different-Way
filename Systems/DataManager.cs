using System;
using System.Collections.Generic;
using System.Text.Json;
using Godot;
using DifferentWay.Systems.Models;
using DifferentWay.Core;

namespace DifferentWay.Systems;

public static class DataManager
{
    public static Dictionary<string, MobData> Mobs { get; private set; } = new();
    public static Dictionary<string, WeaponData> Weapons { get; private set; } = new();
    public static Dictionary<string, ArmorData> Armors { get; private set; } = new();
    public static Dictionary<string, SkillData> Skills { get; private set; } = new();
    public static Dictionary<string, ConsumableData> Consumables { get; private set; } = new();
    public static Dictionary<string, MaterialData> Materials { get; private set; } = new();
    public static Dictionary<string, StatusEffectData> StatusEffects { get; private set; } = new();

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public static void Initialize()
    {
        try
        {
            var mobsList = LoadJson<List<MobData>>("res://Data/Mobs/bestiary.json");
            foreach (var mob in mobsList) Mobs[mob.Id] = mob;

            var weaponsList = LoadJson<List<WeaponData>>("res://Data/Items/Weapons/weapons.json");
            foreach (var w in weaponsList) Weapons[w.Name] = w;

            var armorsList = LoadJson<List<ArmorData>>("res://Data/Items/Armor/armor.json");
            foreach (var a in armorsList) Armors[a.Name] = a;

            var skillsList = LoadJson<List<SkillData>>("res://Data/Skills/skills.json");
            foreach (var s in skillsList) Skills[s.Name] = s;

            var consumablesList = LoadJson<List<ConsumableData>>("res://Data/Items/Consumables/consumables.json");
            foreach (var c in consumablesList) Consumables[c.Name] = c;

            var materialsList = LoadJson<List<MaterialData>>("res://Data/Items/Materials/materials.json");
            foreach (var m in materialsList) Materials[m.Name] = m;

            var effectsList = LoadJson<List<StatusEffectData>>("res://Data/StatusEffects/status_effects.json");
            foreach (var e in effectsList) StatusEffects[e.Name] = e;

            GameLogger.Log("DataManager initialized successfully. Loaded core game data.");
        }
        catch (Exception ex)
        {
            GameLogger.LogError($"Failed to initialize DataManager: {ex.Message}");
        }
    }

    public static T LoadJson<T>(string godotPath)
    {
        using var file = Godot.FileAccess.Open(godotPath, Godot.FileAccess.ModeFlags.Read);
        if (file == null)
        {
            throw new Exception($"Failed to open file at path: {godotPath}");
        }

        string jsonText = file.GetAsText();
        var result = JsonSerializer.Deserialize<T>(jsonText, JsonOptions);

        if (result == null)
        {
            throw new Exception($"Failed to deserialize JSON from path: {godotPath}");
        }

        return result;
    }
}
