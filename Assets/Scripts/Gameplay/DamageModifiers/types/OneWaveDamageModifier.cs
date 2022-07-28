using System.Collections.Generic;
using System.Linq;

public class OneWaveDamageModifier : IDamageModifier
{
    private readonly Dictionary<DamageType, float> _modifications;

    public OneWaveDamageModifier()
    {
        _modifications = GameUtils.DefaultModifications();
    }

    public Dictionary<DamageType, float> GetModifications() => new(_modifications);

    public void AnalyzeWaveStats(ICollector collector)
    {
        Dictionary<DamageType, int> stats = collector.GetStats();
        int total = stats.Values.Sum();
        float defaultPercentage = 0.1f;
        foreach (var stat in stats)
        {
            float modifier = (float)(total - stat.Value) / total;
            if (modifier == 0) modifier = defaultPercentage;
            _modifications[stat.Key] = GameUtils.FormatModifierValue(modifier);
        }
    }

    public void ResetModifications()
    {
        foreach (var defaultModifier in GameUtils.DefaultModifications())
            _modifications[defaultModifier.Key] = defaultModifier.Value;
    }
}
