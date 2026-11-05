using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class TouchControls : MonoBehaviour
{
    [Header("Layout")]

    [SerializeField] private GameObject _controls;

    // Everything that sits by a screen edge, kept clear of notches and rounded corners
    [SerializeField] private RectTransform _safeArea;

    [Header("Controls")]

    [SerializeField] private TouchStick _moveStick;

    [SerializeField] private TouchLookArea _lookArea;

    [SerializeField] private TouchButton _jump;

    [SerializeField] private TouchButton _attack;

    [SerializeField] private TouchButton _pause;

    [SerializeField] private TouchWeaponCross _weaponCross;

    [SerializeField] private Image _attackIcon;

    [Header("Weapons")]

    [SerializeField] private TouchWeaponView _fist;

    [SerializeField] private TouchWeaponView _machete;

    [SerializeField] private TouchWeaponView _crossbow;

    [SerializeField] private TouchWeaponView _grenade;

    [Inject] private readonly TouchMode _touchMode;

    private Dictionary<string, TouchWeaponView> _weapons;

    private Rect _appliedSafeArea;

    public TouchStick MoveStick { get => _moveStick; }

    public TouchLookArea LookArea { get => _lookArea; }

    public TouchButton Jump { get => _jump; }

    public TouchButton Attack { get => _attack; }

    public TouchButton Pause { get => _pause; }

    public TouchWeaponCross WeaponCross { get => _weaponCross; }

    public void ShowWeapon(string label)
    {
        if (!_weapons.TryGetValue(label, out TouchWeaponView selected)) return;
        _attackIcon.sprite = selected.Icon;
        foreach (var weapon in _weapons) weapon.Value.Highlight.SetActive(weapon.Key == label);
    }

    private void Awake()
    {
        _weapons = new() {
            {"unarmed", _fist},
            {"machete", _machete},
            {"crossbow", _crossbow},
            {"grenade", _grenade},
        };
    }

    private void OnEnable()
    {
        _touchMode.Changed += setVisible;
        setVisible(_touchMode.IsActive);
    }

    private void OnDisable() => _touchMode.Changed -= setVisible;

    private void Update()
    {
        if (_touchMode.IsActive && Screen.safeArea != _appliedSafeArea) applySafeArea();
    }

    private void setVisible(bool isVisible) => _controls.SetActive(isVisible);

    private void applySafeArea()
    {
        _appliedSafeArea = Screen.safeArea;
        var screen = new Vector2(Screen.width, Screen.height);
        _safeArea.anchorMin = _appliedSafeArea.min / screen;
        _safeArea.anchorMax = _appliedSafeArea.max / screen;
    }
}
