using System.Collections.Generic;

public interface IDamageModifier
{
    Dictionary<DamageType, float> GetModifications();

    void AnalyzeWaveStats(ICollector collector);

    void ResetModifications();
}
