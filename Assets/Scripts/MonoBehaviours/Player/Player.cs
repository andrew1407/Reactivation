using System.Collections;
using UnityEngine;
using Zenject;

public class Player : MonoBehaviour, IDamageTaker
{
    [SerializeField] private GameObject _deathEffectPrefab;

    [SerializeField] private Transform _head;

    [SerializeField] private CharacterController _characterController;

    [Inject] private readonly CharacterComponents _characterComponents;

    [Inject] private readonly PlayerHealthController _healthController;

    [Inject] private readonly HealthContainer _healthContainer;

    [Inject] private readonly PlayerWeaponSelector _weaponSelector;

    [Inject] private readonly PlayerActivityObserver _activityObserver;

    [Inject] private readonly UiController _uiController;

    [Inject] private readonly CameraMovement _cameraMovement;

    [Inject] private readonly PlayerMotionControl _motionControl;

    [Inject(Id = "ArrowManager")] private readonly IAmmoManager _arrowManager;

    [Inject(Id = "GrenadeManager")] private readonly IAmmoManager _grenadeManager;

    [Inject] private readonly IAmmoContainer[] _ammoContainers;

    private const float _deathEffectCleanup = 5;

    public Transform Head { get => _head; }

    public void TakeDamage(DamageType damageType, float damage)
    {
        _healthController.TakeDamage(damageType, damage);
        if (_healthContainer.HealthPoints == 0)
            _activityObserver.OnWaveEnd(WaveState.DIED);
    }

    public void Place(Transform place)
    {
        _characterController.enabled = false;
        gameObject.transform.position = place.position;
        gameObject.transform.rotation = place.rotation;
        _characterController.enabled = true;
    }

    public void Die()
    {
        StartCoroutine(onDeath());
    }

    public void Resurrect()
    {
        setInitialHealthParams();
        _cameraMovement.CameraValues = Vector2.zero;
        gameObject.SetActive(true);
        StartCoroutine(_weaponSelector.SetUnarmedState());
    }

    public void ResetStats()
    {
        foreach (var weapon in _ammoContainers) weapon.RefillAmmo();
        _healthContainer.SetMaxHealth();
        setInitialHealthParams();
        setInitialAmmoParams();
    }

    
    private void Start()
    {
        setInitialHealthParams();
        setInitialAmmoParams();
    }

    private void OnAnimatorMove() => _motionControl.UpdateRootMotion();

    private void OnControllerColliderHit(ControllerColliderHit hit) => _motionControl.PushObstacle(hit);

    private IEnumerator onDeath()
    {
        yield return _weaponSelector.ResetAllWeapons();
        gameObject.SetActive(false);
        _healthController.RegenerationActive = false;
        GameObject deathEfect = Instantiate(_deathEffectPrefab, transform.position, Quaternion.identity);
        Destroy(deathEfect, _deathEffectCleanup);
    }

    private void setInitialAmmoParams()
    {
        _uiController.SetAmmo(label: "crossbow", _arrowManager.Count);
        _uiController.SetAmmo(label: "grenade", _grenadeManager.Count);
    }

    private void setInitialHealthParams()
    {
        _healthController.RegenerationActive = true;
        _uiController.Health = _healthContainer.HealthPoints;
    }

    public class Factory : PlaceholderFactory<Player> {}
}
