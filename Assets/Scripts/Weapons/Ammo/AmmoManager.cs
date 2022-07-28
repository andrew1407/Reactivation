using UnityEngine;
using Zenject;

public class AmmoManager : IAmmoManager
{
    private ObjectPool<IAmmo> _pool;

    private AmmoParams _ammoParams;

    [Inject(Id = "PreloadPosition")] private Transform _preloadPosition;

    [Inject] private DiContainer _diContainer;

    [Inject]
    private void Constuct(AmmoParams ammoParams)
    {
        _ammoParams = ammoParams;
        _pool = new(_ammoParams.Count, createAmmo);
    }

    public int Count { get => _pool.Size; }

    public int Left { get => _pool.Free; }

    public IAmmo GetFromPool()
    {
        IAmmo ammo = _pool.Give();
        if (ammo != null) ammo.BindState(interractive: true, _ammoParams.Holder);
        return ammo; 
    }

    public bool ReturnToPool(IAmmo ammo)
    {
        bool put = _pool.Put(ammo);
        if (put) ammo.BindState(interractive: false, _preloadPosition);
        return put;        
    }

    public void Refill()
    {
        foreach (var ammo in _pool.GetReserved()) ReturnToPool(ammo);
    }

    private IAmmo createAmmo()
    {
        GameObject created = _diContainer.InstantiatePrefab(_ammoParams.Prebab);
        var ammo = created.GetComponent<IAmmo>();
        ammo.BindState(interractive: false, _preloadPosition);
        return ammo;
    }
}
