using System.Collections;
using UnityEngine;
using Zenject;

public class GrenadeHolder : FirearmsWeapon
{
    [Inject] private readonly CharacterComponents _characterComponents;

    private readonly int _actionHash = Animator.StringToHash("throw_grenade");

    private readonly int _idleHash = Animator.StringToHash("idle");

    private Vector3 _force;

    protected override Vector3 _forceDirection { get => _characterComponents.Transform.TransformDirection(_force); }

    [Inject]
    private void Construct([Inject(Id = "GrenadeManager")] IAmmoManager ammoManager)
    {
        Label = "grenade";
        _ammoManager = ammoManager;
        var throwingAngle = Quaternion.AngleAxis(angle: 50, Vector3.left);
        _force = throwingAngle * Vector3.forward * _ammoAppliance.LaunchForce;
    }

    public override IEnumerator Action()
    {
        Animator animator = _characterComponents.RigAnimator;
        yield return GameUtils.PlayAnimation(animator, _actionHash);
        animator.Play(_idleHash);
    }

    public void OnAnimationTrigger() => useAmmo();
}
