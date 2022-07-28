using UnityEngine;
using Zenject;
using TMPro;

public class EnemyHealthController
{
    [Inject] private readonly TextMeshProUGUI _healtText;

    [Inject] private readonly RectTransform _tab;

    [Inject] private readonly HealthContainer _healthContainer;

    [Inject] private readonly DamageModifiersSystem _modifiersSystem;

    private Transform _transform;

    [Inject]
    private void Construct(CharacterComponents characterComponents)
    {
        _transform = characterComponents.Transform;
    }

    public void SetInitialHealth()
    {
        _healthContainer.SetMaxHealth();
        setTabValue();
    }

    public void OnDamage(DamageType damageType, float damage)
    {
        float modifiedDamage = _modifiersSystem.ModifyDamage(damageType, damage);
        _healthContainer.ModifyPoints(-modifiedDamage);
        setTabValue();
    }

    public void RotateHealthTab(Vector3 position)
    {
        _tab.rotation = Quaternion.LookRotation(_transform.position - position);
    }

    private void setTabValue() => _healtText.text = _healthContainer.HealthPoints.ToString();
}
