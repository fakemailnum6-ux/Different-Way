using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using DifferentWay.Systems.DataModels;

namespace DifferentWay.Systems
{
    public partial class DataManager : Node
    {
        public Dictionary<string, WeaponData> Weapons { get; private set; } = new Dictionary<string, WeaponData>();
        public Dictionary<string, ArmorData> Armors { get; private set; } = new Dictionary<string, ArmorData>();
        public Dictionary<string, ConsumableData> Consumables { get; private set; } = new Dictionary<string, ConsumableData>();
        public Dictionary<string, MaterialData> Materials { get; private set; } = new Dictionary<string, MaterialData>();
        public Dictionary<string, MobData> Mobs { get; private set; } = new Dictionary<string, MobData>();
        public Dictionary<string, SkillData> Skills { get; private set; } = new Dictionary<string, SkillData>();
        public Dictionary<string, StatusEffectData> StatusEffects { get; private set; } = new Dictionary<string, StatusEffectData>();

        public override void _Ready()
        {
            Initialize();
        }

        public void Initialize()
        {
            LoadData("res://Data/Items/Weapons/weapons.json", Weapons);
            LoadData("res://Data/Items/Armor/armor.json", Armors);
            LoadData("res://Data/Items/Consumables/consumables.json", Consumables);
            LoadData("res://Data/Items/Materials/materials.json", Materials);
            LoadData("res://Data/Mobs/mobs.json", Mobs);
            LoadData("res://Data/Skills/skills.json", Skills);
            LoadData("res://Data/StatusEffects/status_effects.json", StatusEffects);
            GD.Print("DataManager initialized.");
        }

        private void LoadData<T>(string godotPath, Dictionary<string, T> dictionary)
        {
            // We use Godot's FileAccess to read res:// paths
            using var file = Godot.FileAccess.Open(godotPath, Godot.FileAccess.ModeFlags.Read);
            if (file == null)
            {
                GD.PrintErr($"Failed to load data from {godotPath}. File not found.");
                return;
            }

            string jsonContent = file.GetAsText();
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var items = JsonSerializer.Deserialize<List<T>>(jsonContent, options);

                if (items != null)
                {
                    foreach (var item in items)
                    {
                        var idProp = typeof(T).GetProperty("Id");
                        if (idProp != null)
                        {
                            string id = idProp.GetValue(item) as string;
                            if (!string.IsNullOrEmpty(id))
                            {
                                dictionary[id] = item;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error parsing JSON from {godotPath}: {e.Message}");
            }
        }
    }
}
