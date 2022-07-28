using UnityEngine;
using Zenject;

public class Arrow : PlayerAmmo
{
    [Header("Params")]

    [SerializeField] private float _damage = 50;

    [Inject]
    private void Construct([Inject(Id = "ArrowManager")] IAmmoManager ammoManager)
    {
        _label = "crossbow";
        _ammoManager = ammoManager;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_state != ItemState.FREE || _state == ItemState.USED) return;
        _state = ItemState.USED;
        setComponentsState(state: false);
        _rigidBody.velocity = _rigidBody.angularVelocity = Vector3.zero;
        transform.SetParent(collision.transform);

        GameObject obj = collision.gameObject;
        var damageTaker = obj.GetComponent<IDamageTaker>();
        bool sameLayer = obj.layer == _characterComponents.Layer;
        if (damageTaker != null && !sameLayer) damageTaker.TakeDamage(DamageType.ARROW, _damage);

        _state = ItemState.CLEANUP_AWAIT;
    }
}
