using System.Collections.Generic;
using UnityEngine;

public class WeaponAttackAnimation : MonoBehaviour
{
    [SerializeField]
    private BoxCollider _hitBox;

    private readonly Dictionary<string, bool> _colliderActivity = new() {
        {"attack_start", true},
        {"attack_end", false},
    };

    protected void onWeaponAttackAnimation(string eventName)
    {
        _hitBox.enabled = _colliderActivity[eventName];
    }
}
