using UnityEngine;
using Zenject;

public class PlayerHealthController: ITickable 
{
    [Inject(Id = "RegenerationDelay")] private readonly float _regenerationDelay;

    [Inject(Id = "RegenerationPoints")] private readonly int _regenerationPoints;

    [Inject] private readonly HealthContainer _healthContainer;

    [Inject] private readonly PlayerStatsObserver _statsObserver;

    private float _delayAccumulator = 0;

    private bool _regenerating = false;

    public bool RegenerationActive
    {
        get => _regenerating;
        set
        {
            _regenerating = value;
            _delayAccumulator = 0;
        }
    }

    public void Tick()
    {
        if (!RegenerationActive) return;
        updateRegeneration();
    }

    public void TakeDamage(DamageType damageType, float damage)
    {
        _healthContainer.ModifyPoints(-damage);
        _statsObserver.OnHealthChange(_healthContainer.HealthPoints, damage: true);
    }

    private void updateRegeneration() 
    {
        _delayAccumulator += Time.deltaTime;
        if (_delayAccumulator >=_regenerationDelay)
        {
            _healthContainer.ModifyPoints(_regenerationPoints);
            _statsObserver.OnHealthChange(_healthContainer.HealthPoints, damage: false);
            _delayAccumulator = 0;
        }
    }
}
