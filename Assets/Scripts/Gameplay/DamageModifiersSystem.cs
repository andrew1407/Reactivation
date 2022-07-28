using UnityEngine;
using Zenject;

public class DamageModifiersSystem : ILateDisposable
{
    [Inject] private readonly IDamageModifier[] _damageModifiers;

    [Inject] private readonly UiController _uiController;

    private IDamageModifier _pickedModifier = null;

    public void PickModifier()
    {
        IDamageModifier previous = _pickedModifier;
        do
        {
            int index = Random.Range(0, _damageModifiers.Length);
            _pickedModifier = _damageModifiers[index];
        }
        while(_pickedModifier == previous);
    }

    public float ModifyDamage(DamageType damageType, float damage)
    {
        var modifications = _pickedModifier.GetModifications();
        if (!modifications.ContainsKey(damageType)) return damage;
        return damage * modifications[damageType];
    }

    public void ShowModifications() => _uiController.Modifiers = _pickedModifier?.GetModifications() ?? GameUtils.DefaultModifications();

    public void UseCollectorData(ICollector collector) => _pickedModifier.AnalyzeWaveStats(collector);

    public void ClearModifierData() => _pickedModifier?.ResetModifications();

    public void LateDispose()
    {
        foreach (var modifier in _damageModifiers)
            if (modifier is System.IDisposable d) d.Dispose();
    }
}
