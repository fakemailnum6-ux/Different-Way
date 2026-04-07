using System;
using Godot;

namespace DifferentWay.Systems;

public partial class StatManager : RefCounted
{
    // Base Stats
    public int STR { get; set; } = 10;
    public int DEX { get; set; } = 10;
    public int END { get; set; } = 10;
    public int INT { get; set; } = 10;
    public int Luck { get; set; } = 10;
    public int Charisma { get; set; } = 10;

    public int Karma { get; set; } = 0; // -100 to 100
    public int MentalResistance { get; set; } = 0;
    public bool HasTrauma { get; set; } = false;

    // Derived Stats based on Arc.md formulas
    public int MaxHP => (END * 5) + (STR * 2);
    public int MaxStamina => (END * 5) + (DEX * 2);
    public int MaxMana => (INT * 5);

    public float Evasion => (float)(DEX * 1.5 + Luck * 0.2);
    public float Cardio => (float)(END + STR * 0.5);

    // Current Pools
    public int CurrentHP { get; set; }
    public int CurrentStamina { get; set; }
    public int CurrentMana { get; set; }

    public StatManager()
    {
        CurrentHP = MaxHP;
        CurrentStamina = MaxStamina;
        CurrentMana = MaxMana;
    }

    public void ApplyDamage(int amount)
    {
        CurrentHP = Math.Max(0, CurrentHP - amount);
    }

    // GDScript Exposed Getters
    public int GetSTR() => STR;
    public int GetDEX() => DEX;
    public int GetEND() => END;
    public int GetINT() => INT;
    public int GetLuck() => Luck;
    public int GetCharisma() => Charisma;

    public int GetMaxHP() => MaxHP;
    public int GetCurrentHP() => CurrentHP;
    public int GetMaxStamina() => MaxStamina;
    public int GetCurrentStamina() => CurrentStamina;
    public int GetMaxMana() => MaxMana;
    public int GetCurrentMana() => CurrentMana;

    public float GetEvasion() => Evasion;
    public float GetCardio() => Cardio;
    public int GetMentalResistance() => MentalResistance;
}
