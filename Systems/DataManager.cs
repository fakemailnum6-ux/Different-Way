using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

public partial class DataManager : RefCounted
{
    public Dictionary<string, MobData> Mobs { get; private set; } = new Dictionary<string, MobData>();
    public Dictionary<string, WeaponData> Weapons { get; private set; } = new Dictionary<string, WeaponData>();
    public Dictionary<string, ArmorData> Armors { get; private set; } = new Dictionary<string, ArmorData>();
    public Dictionary<string, SkillData> Skills { get; private set; } = new Dictionary<string, SkillData>();
    public Dictionary<string, ConsumableData> Consumables { get; private set; } = new Dictionary<string, ConsumableData>();
    public Dictionary<string, MaterialData> Materials { get; private set; } = new Dictionary<string, MaterialData>();

    public async Task InitializeAsync()
    {
        try
        {
            GD.Print("DataManager: Starting asynchronous data load...");
            await Task.Run(() => LoadAllData());
            GD.Print("DataManager: Data load complete.");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"DataManager: Failed to initialize. {ex.Message}");
        }
    }

    private void LoadAllData()
    {
        Mobs = LoadJsonToDictionary<MobData>("res://Data/Mobs/mobs.json");
        Weapons = LoadJsonToDictionary<WeaponData>("res://Data/Items/Weapons/weapons.json");
        Armors = LoadJsonToDictionary<ArmorData>("res://Data/Items/Armor/armor.json");
        Skills = LoadJsonToDictionary<SkillData>("res://Data/Skills/skills.json");
        Consumables = LoadJsonToDictionary<ConsumableData>("res://Data/Items/Consumables/consumables.json");
        Materials = LoadJsonToDictionary<MaterialData>("res://Data/Items/Materials/materials.json");
    }

    private Dictionary<string, T> LoadJsonToDictionary<T>(string godotPath) where T : class, IBaseData
    {
        var dict = new Dictionary<string, T>();

        if (!FileAccess.FileExists(godotPath))
        {
            GD.PrintErr($"DataManager: File not found at {godotPath}");
            return dict;
        }

        try
        {
            using var file = FileAccess.Open(godotPath, FileAccess.ModeFlags.Read);
            string json = file.GetAsText();
            var list = JsonConvert.DeserializeObject<List<T>>(json);

            if (list == null) return dict;

            foreach (var item in list)
            {
                if (string.IsNullOrWhiteSpace(item.Id))
                {
                    GD.PrintErr($"DataManager: Validation failed. Found object without an ID in {godotPath}");
                    continue;
                }

                dict[item.Id] = item;
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"DataManager: Error parsing {godotPath}: {ex.Message}");
        }

        return dict;
    }
}
