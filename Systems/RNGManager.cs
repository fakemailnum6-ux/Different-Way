using Godot;
using System;

public static class RNGManager
{
    private static Random _rng = new Random();
    public static int CurrentSeed { get; private set; }

    /// <summary>
    /// Initializes the deterministic RNG sequence.
    /// </summary>
    public static void Initialize(int seed)
    {
        CurrentSeed = seed;
        _rng = new Random(seed);
        GD.Print($"[RNGManager] Initialized with seed {seed}");
    }

    /// <summary>
    /// Returns a random integer between min (inclusive) and max (inclusive).
    /// </summary>
    public static int RollInt(int min, int max)
    {
        // Random.Next is exclusive on the upper bound, so we add 1.
        return _rng.Next(min, max + 1);
    }

    /// <summary>
    /// Returns a random double between min (inclusive) and max (exclusive).
    /// </summary>
    public static double RollDouble(double min, double max)
    {
        return min + (_rng.NextDouble() * (max - min));
    }
}
