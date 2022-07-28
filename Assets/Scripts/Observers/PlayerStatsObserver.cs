using UnityEngine.Events;
using Zenject;

public class PlayerStatsObserver
{
    [Inject] private readonly UiController _uiController;

    private readonly UnityEvent<int, bool> _onHealthChange = new();

    public PlayerStatsObserver()
    {
        _onHealthChange.AddListener(showWeapon);
    }

    public void OnHealthChange(int hp, bool damage) => _onHealthChange.Invoke(hp, damage);

    private void showWeapon(int hp, bool _)
    {
        _uiController.Health = hp;
    }
}
