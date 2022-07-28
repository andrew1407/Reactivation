using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PlayerWeaopnInstaller : MonoInstaller
{
    private enum WeaponTypes
    {
        UNARMED,
        MACHETE,
        CROSSBOW,
        GRENADE,
    }

    private readonly Dictionary<WeaponTypes, Type> _weaponTypes = new() {
        {WeaponTypes.UNARMED, typeof(Unarmed)},
        {WeaponTypes.MACHETE, typeof(Machete)},
        {WeaponTypes.CROSSBOW, typeof(Crossbow)},
        {WeaponTypes.GRENADE, typeof(GrenadeHolder)},
    };

    [SerializeField] private WeaponTypes[] _weaponOrder;

    [Header("Crossbow")]

    [SerializeField] private RecoilParams _crossbowRecoil;

    [SerializeField] private AmmoParams _arrowsAmmo;

    [SerializeField] private AmmoAppliance _arrowAppliance;

    [Header("Grenades")]

    [SerializeField] private AmmoParams _grenadesAmmo;

    [SerializeField] private AmmoAppliance _grenadeAppliance;

    public override void InstallBindings()
    {
        bindAmmoManager<Arrow, Crossbow>(id: "ArrowManager", _arrowsAmmo);
        bindAmmoManager<Grenade, GrenadeHolder>(id: "GrenadeManager", _grenadesAmmo);

        Container.BindInterfacesAndSelfTo<WeaponRecoil>().AsSingle();

        Container.BindInterfacesAndSelfTo<Unarmed>().AsSingle();
        Container.BindInterfacesAndSelfTo<Machete>().AsSingle();
        bindAmmoWeapon<Crossbow>(_arrowAppliance, _crossbowRecoil);
        bindAmmoWeapon<GrenadeHolder>(_grenadeAppliance);

        Container.Bind<PlayerWeaponSelector>()
            .FromSubContainerResolve()
            .ByMethod(installWeaponSelector)
            .AsSingle();
    }

    private void bindAmmoManager<V, H>(string id, AmmoParams ammoParams)
    {
        Container.Bind<IAmmoManager>()
            .WithId(id)
            .FromSubContainerResolve()
            .ByMethod(subc =>
            {
                subc.Bind<IAmmoManager>().To<AmmoManager>().AsSingle();
                subc.BindInstance(ammoParams);
            })
            .AsCached();
        
        Container.BindInstance(ammoParams).AsCached().WhenInjectedInto(typeof(V), typeof(H));
    }

    private void bindAmmoWeapon<T>(AmmoAppliance ammoAppliance, RecoilParams? recoilParams = null)
    {
        Container.BindInterfacesAndSelfTo<T>()
            .FromSubContainerResolve()
            .ByMethod(subc =>
            {
                subc.Bind<T>().AsSingle();
                subc.BindInstance(ammoAppliance);
                if (recoilParams is RecoilParams rp) subc.BindInstance(rp);
            })
            .AsSingle();
    }

    private void installWeaponSelector(DiContainer subContainer)
    {
        subContainer.Bind<PlayerWeaponSelector>().AsSingle();
        Type[] orderedWeapons = _weaponOrder.Select(t => _weaponTypes[t]).ToArray();
        subContainer.BindInstance(orderedWeapons);
    }
}