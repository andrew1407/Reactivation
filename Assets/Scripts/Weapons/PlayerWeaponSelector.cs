using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PlayerWeaponSelector
{
    private struct AnimationsHash
    {
        public int SelectHash;

        public int HideHash;
    }

    [Inject] private readonly WeaponObserver _weaponObserver;

    private readonly Dictionary<IWeapon, AnimationsHash> _weaponsData = new();

    private readonly int _resetHash = Animator.StringToHash("reset_weapons");

    private readonly int _unarmedtHash = Animator.StringToHash("unarmed");

    private readonly int _idleHash = Animator.StringToHash("idle");

    private Animator _animator;

    private IWeapon _unarmed;
    
    public IWeapon SelectedWeapon { get; private set; }

    [Inject]
    private void Construct(CharacterComponents characterComponents, List<IWeapon> weapons, Type[] weaponsOrder)
    {
        _animator = characterComponents.RigAnimator;
        foreach (var weaponType in weaponsOrder)
            addWeaponData(weapons.Find(w => weaponType == w.GetType()));
        SelectedWeapon = _unarmed;
        _weaponObserver.OnWeaponSelect(SelectedWeapon.Label);
    }

    public IEnumerator SwitchWeapon(int index)
    {
        IWeapon weapon = _weaponsData.Keys.ElementAt(index);
        bool currentlyUnarmed = SelectedWeapon == _unarmed;
        bool switchedUnarmed = weapon == _unarmed;
        if (currentlyUnarmed && switchedUnarmed) yield break;
        bool sameSwitch = weapon == SelectedWeapon;
        if (!currentlyUnarmed || sameSwitch)
        {
            IWeapon hidden = SelectedWeapon;
            yield return triggerUsage(_weaponsData[SelectedWeapon].HideHash, _unarmed);
            _weaponObserver.OnWeaponnHide(hidden.Label);
        }
        if (!switchedUnarmed && !sameSwitch)
            yield return triggerUsage(_weaponsData[weapon].SelectHash, weapon);
        _weaponObserver.OnWeaponSelect(SelectedWeapon.Label);
    }

    public IEnumerator SetNext() => selectNearest(offset: 1);

    public IEnumerator SetPrevious() => selectNearest(offset: -1);

    public IEnumerator SetUnarmedState()
    {
        GameUtils.PlayAnimation(_animator, _unarmedtHash);
        yield return null;
        SelectedWeapon = _unarmed;
        _weaponObserver.OnWeaponSelect(SelectedWeapon.Label);
    }

    public IEnumerator ResetAllWeapons()
    {
        GameUtils.PlayAnimation(_animator, _resetHash);
        yield return null;
    }

    private IEnumerator selectNearest(int offset)
    {
        int waponsCount = _weaponsData.Count;
        int currentPosition = Array.IndexOf(_weaponsData.Keys.ToArray(), SelectedWeapon);
        int next = currentPosition + offset;
        yield return SwitchWeapon(next < 0 ? next + waponsCount : next % waponsCount);
    }

    private void addWeaponData(IWeapon weapon)
    {
        bool unarmedType = weapon is Unarmed;
        if (unarmedType) _unarmed = weapon;
        string weaponLabel = weapon.Label;
        _weaponsData[weapon] = unarmedType ? default : new() {
            SelectHash = Animator.StringToHash("select_" + weaponLabel),
            HideHash = Animator.StringToHash("hide_" + weaponLabel),
        };
    }

    private IEnumerator triggerUsage(int hashName, IWeapon next)
    {
        yield return GameUtils.PlayAnimation(_animator, hashName);
        SelectedWeapon = next;
    }
}
