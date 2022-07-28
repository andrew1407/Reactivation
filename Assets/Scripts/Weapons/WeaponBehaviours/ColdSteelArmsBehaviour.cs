using UnityEngine;
using Zenject;

public class ColdSteelArmsBehaviour : MonoBehaviour
{
    [SerializeField] private float _pushForce = 1;

    [SerializeField] private float _damage = 1;

    [SerializeField] private DamageType _damageType;

    [Inject] private readonly CharacterComponents _characterComponents;

    private void OnTriggerEnter(Collider collider)
    {
        Rigidbody rb = collider.attachedRigidbody;
        var obj = collider.gameObject;
        if (rb != null && !rb.isKinematic)
        {
            var forceDirection = obj.transform.position - transform.position;
            forceDirection.Normalize();
            rb.AddForceAtPosition(forceDirection * _pushForce, transform.position, ForceMode.Impulse);
        }
        var damageTaker = obj.GetComponent<IDamageTaker>();
        bool sameLayer = obj.layer == _characterComponents.Layer;
        if (damageTaker != null && !sameLayer) damageTaker.TakeDamage(_damageType, _damage);
    }
}
