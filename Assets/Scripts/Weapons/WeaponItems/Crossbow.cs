using System.Collections;
using UnityEngine;
using Zenject;

public class Crossbow : FirearmsWeapon
{
    [Inject] private readonly AmmoParams _ammoParams;

    [Inject] private readonly CharacterComponents _characterComponents;

    [Inject] private readonly RecoilParams _recoilParams;

    [Inject] private readonly WeaponRecoil _weaponRecoil;

    private Vector3 _force;

    protected override Vector3 _forceDirection { get => _ammoParams.Holder.TransformDirection(_force); }

    [Inject]
    private void Construct([Inject(Id = "ArrowManager")] IAmmoManager ammoManager)
    {
        Label = "crossbow";
        _ammoManager = ammoManager;
        _force = Vector3.forward * _ammoAppliance.LaunchForce;
    }

    public override IEnumerator Action()
    {
        if (useAmmo())
        {
            _weaponRecoil.Activate(_recoilParams);
            yield return null;
        }
    }
}
