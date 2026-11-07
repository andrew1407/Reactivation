using System.Collections.Generic;
using System.Linq;
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

    [SerializeField] private TouchButton _aim;

    [SerializeField] private TouchButton _pause;

    [SerializeField] private TouchWeaponCross _weaponCross;

    [SerializeField] private Image _attackIcon;

    [SerializeField] private GameObject _aimHighlight;

    [Header("Pause")]

    [SerializeField] private Image _pauseIcon;

    [SerializeField] private Sprite _pauseSprite;

    [SerializeField] private Sprite _resumeSprite;

    [Header("Weapons")]

    [SerializeField] private TouchWeaponView _fist;

    [SerializeField] private TouchWeaponView _machete;

    [SerializeField] private TouchWeaponView _crossbow;

    [SerializeField] private TouchWeaponView _grenade;

    [Inject] private readonly TouchMode _touchMode;

    private Dictionary<string, TouchWeaponView> _weapons;

    private Rect _appliedSafeArea;

    private Dictionary<RectTransform, Vector2> _rightControlPositions;

    private float _canvasHeight;

    public TouchStick MoveStick { get => _moveStick; }

    public TouchLookArea LookArea { get => _lookArea; }

    public TouchButton Jump { get => _jump; }

    public TouchButton Attack { get => _attack; }

    public TouchButton Aim { get => _aim; }

    public TouchButton Pause { get => _pause; }

    public TouchWeaponCross WeaponCross { get => _weaponCross; }

    public void ShowWeapon(string label)
    {
        if (!_weapons.TryGetValue(label, out TouchWeaponView selected)) return;
        _attackIcon.sprite = selected.Icon;
        foreach (var weapon in _weapons) weapon.Value.Highlight.SetActive(weapon.Key == label);
    }

    public void ShowAiming(bool isAiming)
    {
        // Already gone when this comes from the player being unloaded with the scene
        if (_aimHighlight != null) _aimHighlight.SetActive(isAiming);
    }

    public void ShowPaused(bool isPaused) => _pauseIcon.sprite = isPaused ? _resumeSprite : _pauseSprite;

    private void Awake()
    {
        _weapons = new() {
            {"unarmed", _fist},
            {"machete", _machete},
            {"crossbow", _crossbow},
            {"grenade", _grenade},
        };
        _rightControlPositions = new[] {_jump.transform, _attack.transform, _aim.transform, _weaponCross.transform}
            .Cast<RectTransform>()
            .ToDictionary(rect => rect, rect => rect.anchoredPosition);
        _canvasHeight = GetComponentInParent<CanvasScaler>().referenceResolution.y;
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
        fitRightControls();
    }

    // The canvas follows the screen's height, so a narrow screen has less room for them across. They shrink
    // together towards the bottom of the weapon cross, which keeps it over the weapon's name
    private void fitRightControls()
    {
        var aim = (RectTransform)_aim.transform;
        var cross = (RectTransform)_weaponCross.transform;
        Vector2 origin = _rightControlPositions[cross] + Vector2.down * cross.rect.height / 2;
        float width = _appliedSafeArea.width / Screen.height * _canvasHeight;
        float reach = origin.x - (_rightControlPositions[aim].x - aim.rect.width / 2);
        float stickZoneShare = ((RectTransform)_moveStick.transform).anchorMax.x;
        float scale = TouchUtils.ToStickZoneFit(width, reach, -origin.x, stickZoneShare);

        foreach (var (rect, position) in _rightControlPositions) rect.anchoredPosition = origin + (position - origin) * scale;
        cross.localScale = Vector3.one * scale;
        _jump.Resize(scale);
        _attack.Resize(scale);
        _aim.Resize(scale);
    }
}
