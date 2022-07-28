using UnityEngine;
using Zenject;

public class Grenade : PlayerAmmo
{
    [Header("Params")]

    [SerializeField] private GameObject _explosionEffect;

    [SerializeField] private float _explosionDelay = 2;

    [SerializeField] private float _blastRadius = 2;

    [SerializeField] private float _explosionForce = 2;

    [SerializeField] private float _explosionRadius = 2;

    [SerializeField] private float _damage = 80;

    [Inject]
    private void Construct([Inject(Id = "GrenadeManager")] IAmmoManager ammoManager)
    {
        _label = "grenade";
        _ammoManager = ammoManager;
    }

    private void OnCollisionEnter(Collision _)
    {
        if (_state != ItemState.FREE || _state == ItemState.USED) return;
        _state = ItemState.USED;
        Invoke(nameof(explode), _explosionDelay);
    }
    
    private void explode()
    {
        if (_state != ItemState.USED) return;
        setComponentsState(state: false);
        Instantiate(_explosionEffect, transform.position, transform.rotation);
        _ammoView.SetActive(false);
        
        Collider[] colliders = Physics.OverlapSphere(transform.position, _blastRadius);
        foreach (var obj in colliders)
        {
            Rigidbody rb = obj.attachedRigidbody;
            if (rb != null && !rb.isKinematic)
                rb.AddExplosionForce(_explosionForce, transform.position, _explosionRadius);
            var damageTaker = obj.GetComponent<IDamageTaker>();
            bool sameLayer = obj.gameObject.layer == _characterComponents.Layer;
            if (damageTaker != null && !sameLayer) damageTaker.TakeDamage(DamageType.GRENADE, _damage);
        }

        _state = ItemState.CLEANUP_AWAIT;
    }
}
