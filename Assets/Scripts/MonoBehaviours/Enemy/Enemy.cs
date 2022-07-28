using UnityEngine;
using Zenject;

public class Enemy : MonoBehaviour, IDamageTaker
{
    [SerializeField] private GameObject _deathEffectPrefab;

    [Inject] private CharacterComponents _characterComponents;

    [Inject] private EnemyHealthController _healthController;

    [Inject] private HealthContainer _healthContainer;

    [Inject] private EnemiesActivityObserver _activityObserver;

    [Inject] private EnemyActionsStrategy _actionsStrategy;

    private const float _deathEffectCleanup = 5;

    public void TakeDamage(DamageType damageType, float damage)
    {
        _healthController.OnDamage(damageType, damage);
        if (_healthContainer.HealthPoints == 0)
            _activityObserver.OnEnemyDefeated(this, damageType);
    }

    public void Place(Transform place)
    {
        _actionsStrategy.NavMeshAgentEnabled = false;
        gameObject.transform.position = place.position;
        gameObject.transform.rotation = place.rotation;
        _actionsStrategy.NavMeshAgentEnabled = true;
    }

    public void Die(bool deathEffect = true)
    {
        gameObject.SetActive(false);
        if (!deathEffect) return;
        GameObject deathSmoke = Instantiate(_deathEffectPrefab, transform.position, Quaternion.identity);
        Destroy(deathSmoke, _deathEffectCleanup);
    }

    public void Resurrect()
    {
        _healthController.SetInitialHealth();
        gameObject.SetActive(true);
    }

    public class Factory : PlaceholderFactory<Enemy> {}
}
