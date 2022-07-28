using System.Collections.Generic;
using System.Linq;

public class PlayerKillsCollector : ICollector
{
    private Dictionary<DamageType, int> _defeatedEnemnies = new() {
        {DamageType.FIST, 0},
        {DamageType.MACHETE, 0},
        {DamageType.ARROW, 0},
        {DamageType.GRENADE, 0},
    };

    public Dictionary<DamageType, int> GetStats() => new(_defeatedEnemnies);

    public void AddValue(DamageType damageType)
    {
        if (_defeatedEnemnies.ContainsKey(damageType))
            lock (_defeatedEnemnies) ++_defeatedEnemnies[damageType];
    }

    public void ClearData()
    {
        foreach (var key in _defeatedEnemnies.Keys.ToList()) _defeatedEnemnies[key] = 0;
    }
}
