using UnityEngine.Events;
using Zenject;

public class EnemiesActivityObserver
{
    [Inject] private readonly EnemiesSpawner _enemiesSpawner;

    [Inject] private readonly PlayerActivityObserver _playerActivityObserver;

    [Inject] private readonly WavesController _wavesController;

    [Inject] private readonly PlayerKillsCollector _killsCollector;

    private readonly UnityEvent<Enemy, DamageType> _onEnemyDefeated = new();

    public EnemiesActivityObserver()
    {
        _onEnemyDefeated.AddListener(enemyDealthAction);
    }

    public void OnEnemyDefeated(Enemy enemy, DamageType damageType) => _onEnemyDefeated.Invoke(enemy, damageType);

    private void enemyDealthAction(Enemy enemy, DamageType damageType)
    {
        _enemiesSpawner.DeactivateEnemy(enemy);
        _killsCollector.AddValue(damageType);
        if (_enemiesSpawner.AllDefeated)
        {
            WaveState stateType = _wavesController.FinalWave ? WaveState.ALL_WAVES_SURVIVED : WaveState.WAVE_PASSED;
            _playerActivityObserver.OnWaveEnd(stateType);
        }
    }
}
