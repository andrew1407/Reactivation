using UnityEngine.Events;
using Zenject;

public class PlayerAmmoObserver
{
    [Inject] private readonly UiController _uiController;

    private readonly UnityEvent<string, int> _onAmmoChange = new();

    public PlayerAmmoObserver()
    {
        _onAmmoChange.AddListener(changeUiInfo);
    }

    public void OnAmmoChange(string label, int count) => _onAmmoChange.Invoke(label, count);

    private void changeUiInfo(string label, int count) => _uiController.SetAmmo(label, count);
}
