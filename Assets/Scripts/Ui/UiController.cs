using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UiController : MonoBehaviour
{
    [Header("Player")]

    [SerializeField] private TextMeshProUGUI _health;

    [Header("Waves")]

    [SerializeField] private TextMeshProUGUI _timer;

    [SerializeField] private TextMeshProUGUI _wave;

    [Header("Weapons")]

    [SerializeField] private TextMeshProUGUI _selectedWeapon;

    [SerializeField] private TextMeshProUGUI _arrows;

    [SerializeField] private TextMeshProUGUI _gernades;
    
    [Header("Modifiers")]

    [SerializeField] private TextMeshProUGUI _fistModifier;

    [SerializeField] private TextMeshProUGUI _macheteModifier;

    [SerializeField] private TextMeshProUGUI _arrowsModifier;

    [SerializeField] private TextMeshProUGUI _grenadesModifier;

    private Dictionary<DamageType, TextMeshProUGUI> _modifiers;

    private Dictionary<string, TextMeshProUGUI> _ammo;

    #region Player

    public float Health
    {
        set => _health.text = value.ToString("0");
    }

    #endregion

    #region Waves

    public float Timer
    {
        set => _timer.text = value.ToString("0");
    }

    public uint Wave
    {
        set => _wave.text = value.ToString();
    }

    #endregion

    #region Weapons

    public string SelectedWeapon
    {
        set => _selectedWeapon.text = char.ToUpper(value[0]) + value.Substring(1);
    }

    public void SetAmmo(string label, int value)
    {
        if (_ammo.ContainsKey(label)) _ammo[label].text = value.ToString();
    }

    public int GetAmmo(string label) => int.Parse(_ammo[label].text);

    #endregion

    #region Modifiers

    public Dictionary<DamageType, float> Modifiers
    {
        set
        {
            foreach (var modifier in _modifiers)
            {
                var key = modifier.Key;
                float modifierValue = value.ContainsKey(key) ? value[key] : 1;
                _modifiers[key].text = modifierValue.ToString("0.00");
            }
        }
    }
    
    #endregion
    
    private void Awake()
    {
        _modifiers = new() {
            {DamageType.FIST, _fistModifier},
            {DamageType.MACHETE, _macheteModifier},
            {DamageType.ARROW, _arrowsModifier},
            {DamageType.GRENADE, _grenadesModifier},
        };

        _ammo = new() {
            {"grenade", _gernades},
            {"crossbow", _arrows},
        };
    }
}
