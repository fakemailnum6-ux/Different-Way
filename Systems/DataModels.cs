using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DifferentWay.Systems.DataModels
{
    public class BaseItem
    {
        [JsonPropertyName("id")] public string Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("weight")] public float Weight { get; set; }
        [JsonPropertyName("price")] public int Price { get; set; }
    }

    public class WeaponData : BaseItem
    {
        [JsonPropertyName("damage")] public int Damage { get; set; }
        [JsonPropertyName("durability")] public int Durability { get; set; }
        [JsonPropertyName("max_durability")] public int MaxDurability { get; set; }
        [JsonPropertyName("type")] public string Type { get; set; }
        [JsonPropertyName("break_chance_bonus")] public bool BreakChanceBonus { get; set; }
        [JsonPropertyName("crit_bonus")] public int CritBonus { get; set; }
        [JsonPropertyName("rear_attack_allowed")] public bool RearAttackAllowed { get; set; }
        [JsonPropertyName("armor_penetration")] public int ArmorPenetration { get; set; }
        [JsonPropertyName("shield_damage_mult")] public float ShieldDamageMult { get; set; }
        [JsonPropertyName("req_str")] public int ReqStr { get; set; }
        [JsonPropertyName("ignore_vanguard")] public bool IgnoreVanguard { get; set; }
        [JsonPropertyName("buffs")] public Dictionary<string, int> Buffs { get; set; }
    }

    public class ArmorData : BaseItem
    {
        [JsonPropertyName("armor")] public int Armor { get; set; }
        [JsonPropertyName("durability")] public int Durability { get; set; }
        [JsonPropertyName("max_durability")] public int MaxDurability { get; set; }
        [JsonPropertyName("social_status")] public string SocialStatus { get; set; }
        [JsonPropertyName("flee_stamina_reduction")] public bool FleeStaminaReduction { get; set; }
        [JsonPropertyName("type")] public string Type { get; set; }
        [JsonPropertyName("debuffs")] public Dictionary<string, int> Debuffs { get; set; }
        [JsonPropertyName("buffs")] public Dictionary<string, int> Buffs { get; set; }
    }

    public class ConsumableData : BaseItem
    {
        [JsonPropertyName("effect_hp")] public int EffectHp { get; set; }
        [JsonPropertyName("effect_mana")] public int EffectMana { get; set; }
        [JsonPropertyName("effect_stamina")] public int EffectStamina { get; set; }
        [JsonPropertyName("effect_stamina_full")] public bool EffectStaminaFull { get; set; }
        [JsonPropertyName("combat_usable")] public bool CombatUsable { get; set; } = true;
        [JsonPropertyName("buffs")] public Dictionary<string, int> Buffs { get; set; }
        // For item_bandages and antidote
        [JsonPropertyName("removes_status")] public object RemovesStatus { get; set; } // string or list
        [JsonPropertyName("effect_hp_regen")] public EffectHpRegenData EffectHpRegen { get; set; }
    }

    public class EffectHpRegenData
    {
        [JsonPropertyName("amount")] public int Amount { get; set; }
        [JsonPropertyName("turns")] public int Turns { get; set; }
    }

    public class MaterialData : BaseItem
    {
        [JsonPropertyName("usage")] public object Usage { get; set; } // string or list
        [JsonPropertyName("requires")] public string Requires { get; set; }
    }

    public class MobData
    {
        [JsonPropertyName("id")] public string Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("hp")] public int Hp { get; set; }
        [JsonPropertyName("damage")] public int Damage { get; set; }
        [JsonPropertyName("armor")] public int Armor { get; set; }
        [JsonPropertyName("dex")] public int Dex { get; set; }
        [JsonPropertyName("str")] public int Str { get; set; }
        [JsonPropertyName("is_elite")] public bool IsElite { get; set; }
        [JsonPropertyName("is_undead")] public bool IsUndead { get; set; }
        [JsonPropertyName("traits")] public List<string> Traits { get; set; }
        [JsonPropertyName("drops")] public List<string> Drops { get; set; }
        [JsonPropertyName("weapon")] public string Weapon { get; set; }
        [JsonPropertyName("weakness")] public string Weakness { get; set; }
    }

    public class SkillData
    {
        [JsonPropertyName("id")] public string Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("cost_stamina")] public int CostStamina { get; set; }
        [JsonPropertyName("cost_energy")] public int CostEnergy { get; set; }
        [JsonPropertyName("cost_mana")] public int CostMana { get; set; }
        [JsonPropertyName("effect_damage_mult")] public float EffectDamageMult { get; set; }
        [JsonPropertyName("cooldown")] public int Cooldown { get; set; }
        [JsonPropertyName("status_effect")] public string StatusEffect { get; set; }
        [JsonPropertyName("status_duration")] public int StatusDuration { get; set; }
        [JsonPropertyName("damage_fire")] public int DamageFire { get; set; }
        [JsonPropertyName("burn_chance")] public int BurnChance { get; set; }
        [JsonPropertyName("damage")] public int Damage { get; set; }
        [JsonPropertyName("ignore_traps")] public bool IgnoreTraps { get; set; }
        [JsonPropertyName("ignore_terrain_penalty")] public bool IgnoreTerrainPenalty { get; set; }
        [JsonPropertyName("heal_mult_int")] public int HealMultInt { get; set; }
        [JsonPropertyName("buff_armor")] public int BuffArmor { get; set; }
        [JsonPropertyName("duration")] public int Duration { get; set; }
        [JsonPropertyName("move_to_rear_no_provoke")] public bool MoveToRearNoProvoke { get; set; }
        [JsonPropertyName("debuff_vanguard_dmg_percent")] public int DebuffVanguardDmgPercent { get; set; }
        [JsonPropertyName("damage_arcane")] public int DamageArcane { get; set; }
        [JsonPropertyName("hit_chance")] public int HitChance { get; set; }
        [JsonPropertyName("ignore_evasion")] public bool IgnoreEvasion { get; set; }
    }

    public class StatusEffectData
    {
        [JsonPropertyName("id")] public string Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("type")] public string Type { get; set; }
        [JsonPropertyName("stack_rule")] public string StackRule { get; set; }
        [JsonPropertyName("max_stacks")] public int MaxStacks { get; set; }
        [JsonPropertyName("duration")] public object Duration { get; set; } // int or string "until_rest"
        [JsonPropertyName("effect_hp_turn")] public int EffectHpTurn { get; set; }
        [JsonPropertyName("effect_hp_turn_per_stack")] public int EffectHpTurnPerStack { get; set; }
        [JsonPropertyName("ignore_armor")] public bool IgnoreArmor { get; set; }
        [JsonPropertyName("double_dmg_on_evasion")] public bool DoubleDmgOnEvasion { get; set; }
        [JsonPropertyName("zero_evasion")] public bool ZeroEvasion { get; set; }
        [JsonPropertyName("post_immunity_duration")] public int PostImmunityDuration { get; set; }
        [JsonPropertyName("cancel_by")] public string CancelBy { get; set; }
        [JsonPropertyName("effect_dex")] public int EffectDex { get; set; }
        [JsonPropertyName("effect_str")] public int EffectStr { get; set; }
        [JsonPropertyName("effect_str_per_stack")] public int EffectStrPerStack { get; set; }
        [JsonPropertyName("effect_dex_per_stack")] public int EffectDexPerStack { get; set; }
        [JsonPropertyName("vulnerability")] public string Vulnerability { get; set; }
        [JsonPropertyName("effect_armor_percent")] public int EffectArmorPercent { get; set; }
        [JsonPropertyName("effect_accuracy_percent")] public int EffectAccuracyPercent { get; set; }
        [JsonPropertyName("cardio_regen_mult")] public float CardioRegenMult { get; set; }
        [JsonPropertyName("effect_phys_dmg_percent")] public int EffectPhysDmgPercent { get; set; }
        [JsonPropertyName("block_magic")] public bool BlockMagic { get; set; }
        [JsonPropertyName("block_potions")] public bool BlockPotions { get; set; }
        [JsonPropertyName("map_speed_penalty")] public bool MapSpeedPenalty { get; set; }
    }
}
