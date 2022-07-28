using System.Collections;
using UnityEngine;
using Zenject;

public abstract class FirearmsWeapon : IWeapon, IAmmoContainer
{
    [Inject] protected readonly AmmoAppliance _ammoAppliance;

    [Inject] protected readonly PlayerAmmoObserver _ammoObserver;

    protected IAmmoManager _ammoManager;

    protected IAmmo _selectedAmmo;

    private bool _reloading = false;

    protected abstract Vector3 _forceDirection { get; }

    public string Label { get; protected set; }

    public abstract IEnumerator Action();

    public IEnumerator Reload()
    {
        if (_reloading) yield break;
        _reloading = true;
        yield return takeAmmoDelayed();
        _reloading = false;
    }

    public void RefillAmmo()
    {
        _reloading = false;
        setAmmo(clear: true);
        _ammoManager.Refill();
        setAmmo();
    }

    public void Setup() => setAmmo();

    protected bool useAmmo()
    {
        if (_selectedAmmo == null) return false;
        _selectedAmmo.Use(_forceDirection);
        setAmmo(clear: true);
        return true;
    }

    protected void setAmmo(bool clear = false)
    {
        _selectedAmmo = clear ? null : _ammoManager.GetFromPool();
        int ammoCount = _ammoManager.Left;
        bool gained = !clear && _selectedAmmo != null;
        if (gained) ++ammoCount;
        _ammoObserver.OnAmmoChange(Label, ammoCount);
    }
    
    private IEnumerator takeAmmoDelayed()
    {
        if (_selectedAmmo != null || !_reloading) yield break;
        yield return new WaitForSeconds(_ammoAppliance.RetakePeriod);
        if (_selectedAmmo != null || !_reloading) yield break;
        setAmmo();
        if (_selectedAmmo != null || !_reloading) yield break;
        yield return new WaitUntil(() => _ammoManager.Left > 0);
        if (_selectedAmmo != null || !_reloading) yield break;
        setAmmo();
    }
}
