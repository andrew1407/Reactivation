using System;
using System.Collections.Generic;
using System.Linq;

public class RandomDamageModifier : IDamageModifier
{
    private readonly Random _random = new();
    
    private readonly Dictionary<DamageType, float> _modifications;

    public RandomDamageModifier()
    {
        _modifications = GameUtils.DefaultModifications();
    }

    public Dictionary<DamageType, float> GetModifications() => new(_modifications);

    public void AnalyzeWaveStats(ICollector _)
    {
        foreach (var key in _modifications.Keys.ToList())
            _modifications[key] = GameUtils.FormatModifierValue((float)_random.NextDouble());
    }

    public void ResetModifications()
    {
        foreach (var defaultModifier in GameUtils.DefaultModifications())
            _modifications[defaultModifier.Key] = defaultModifier.Value;
    }
}

