using UnityEngine.Events;
using Zenject;

public class WeaponObserver
{
    [Inject] private readonly UiController _uiController;

    [Inject] private readonly TouchControls _touchControls;

    private readonly UnityEvent<string> _onWeaponnSelect = new();

    private readonly UnityEvent<string> _onWeaponnHide = new();

    public WeaponObserver()
    {
        _onWeaponnSelect.AddListener(showWeapon);
    }

    public void OnWeaponSelect(string label) => _onWeaponnSelect.Invoke(label);

    public void OnWeaponnHide(string label) => _onWeaponnHide.Invoke(label);

    private void showWeapon(string label)
    {
        bool isUnarmed = label == "unarmed";
        _uiController.SelectedWeapon = isUnarmed ? "fist" : label;
        _touchControls.ShowWeapon(label);
    }
}
