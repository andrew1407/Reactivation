using UnityEngine.Events;
using Zenject;

public class PlayerActivityObserver
{
    [Inject] private readonly PlayerSpawner _playerSpawner;

    [Inject] private readonly Timer _timer;

    [Inject] private readonly WavesController _wavesController;

    [Inject] private readonly PlayOptionsSwitch _optionsSwitch;

    [Inject] private readonly WavesStartObserver _wavesSatrtObserver;

    [Inject] private readonly PlayerKillsCollector _killsCollector;

    [Inject] private readonly DamageModifiersSystem _modifiersSystem;

    private readonly UnityEvent<WaveState> _onWaveEnd = new();

    public PlayerActivityObserver()
    {
        _onWaveEnd.AddListener(deactivatePlayer);
        _onWaveEnd.AddListener(handleWinState);
    }

    public void OnWaveEnd(WaveState waveState) => _onWaveEnd.Invoke(waveState);

    private void deactivatePlayer(WaveState waveState)
    {
        if (!GameUtils.PlayerDefeated(waveState)) return;
        _timer.Active = false;
        _killsCollector.ClearData();
        _modifiersSystem.ClearModifierData();
        _playerSpawner.DeactivatePlayer();
        _optionsSwitch.SetOptionState(waveState);
    }

    private void handleWinState(WaveState waveState)
    {
        if (GameUtils.PlayerDefeated(waveState) || !_playerSpawner.PlayerActive) return;
        _timer.Active = false;
        _optionsSwitch.SetOptionState(waveState);
        bool allWavesPassed = waveState == WaveState.ALL_WAVES_SURVIVED;
        if (!allWavesPassed)
        {
            _modifiersSystem.UseCollectorData(_killsCollector);
            _wavesController.SetNextWave();
            _wavesSatrtObserver.OnStart();
        }
        else
        {
            _modifiersSystem.ClearModifierData();
        }
        _killsCollector.ClearData();
    }
}
