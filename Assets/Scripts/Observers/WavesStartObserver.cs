using UnityEngine.Events;
using Zenject;

public class WavesStartObserver
{
    [Inject] private readonly Timer _timer;

    [Inject] private readonly EnemiesSpawner _enemiesSpawner;

    [Inject] private readonly DamageModifiersSystem _modifiersSystem;

    private readonly UnityEvent _onStart = new();

    public WavesStartObserver()
    {
        _onStart.AddListener(setInitialState);
    }

    public void OnStart() => _onStart.Invoke();

    private void setInitialState()
    {
        _modifiersSystem.ShowModifications();
        _timer.Reset();
        _timer.Active = true;
        _enemiesSpawner.Spawn();
    }
}
