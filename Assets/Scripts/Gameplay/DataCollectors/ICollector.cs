using System.Collections.Generic;

public interface ICollector
{
    void AddValue(DamageType damageType);

    Dictionary<DamageType, int> GetStats();

    void ClearData();
}
