using UnityEngine;

public class HealthContainer
{
    private readonly int _maxHealth;

    private float _healthPoints;

    public HealthContainer(int maxHealth)
    {
        _maxHealth = maxHealth;
        SetMaxHealth();
    }

    public int HealthPoints
    {
        get => (int)_healthPoints;
    }

    public void SetMaxHealth() => _healthPoints = _maxHealth;

    public void ModifyPoints(float value)
    {
        lock (this) _healthPoints = Mathf.Clamp(_healthPoints + value, min: 0, max: _maxHealth);
    }
}
