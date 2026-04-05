using System.Collections.Generic;

namespace DifferentWay.Systems;

public class StatusEffect
{
    public string Name { get; set; } = string.Empty;
    public int Duration { get; set; }
}

public class StatusEffectManager
{
    private List<StatusEffect> _activeEffects = new();

    public void ApplyEffect(StatusEffect effect)
    {
        _activeEffects.Add(effect);
    }
}
