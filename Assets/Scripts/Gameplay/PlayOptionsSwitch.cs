using System;
using System.Collections.Generic;
using Zenject;

public class PlayOptionsSwitch
{
    [Inject] private readonly InfoMessageViewer _infoMessageViewer;

    [Inject] private readonly PlayerSpawner _playerSpawner;

    [Inject] private readonly EnemiesSpawner _enemiesSpawner;

    [Inject] private readonly WavesController _wavesController;

    [Inject] private readonly WavesStartObserver _wavesSatrtObserver;

    [Inject] private readonly DamageModifiersSystem _modifiersSystem;

    [Inject] private readonly GameManager _gameManager;

    private readonly Dictionary<WaveState, Action> _optionHandlers;

    private WaveState _currentState = WaveState.NONE;

    public PlayOptionsSwitch()
    {
        _optionHandlers = new() {
            {WaveState.ALL_WAVES_SURVIVED, handleInitialStart},
            {WaveState.IDLE, handleInitialStart},
            {WaveState.DIED, handleLoss},
            {WaveState.TIME_IS_OVER, handleLoss},
        };
    }

    public void SetOptionState(WaveState state)
    {
        _currentState = state;
        _infoMessageViewer.ShowTab(_currentState);
    }

    public void ActivateOption()
    {
        if (_gameManager.GamePaused) return;
        if (!_optionHandlers.ContainsKey(_currentState)) return;
        _optionHandlers[_currentState]();
        SetOptionState(WaveState.NONE);
    }

    private void handleLoss()
    {
        _modifiersSystem.PickModifier();
        _wavesController.ResetParameters();
        _playerSpawner.Respawn();
        _wavesSatrtObserver.OnStart();
    }

    private void handleInitialStart()
    {
        _modifiersSystem.PickModifier();
        _wavesController.ResetParameters();
        _playerSpawner.Player.ResetStats();
        _wavesSatrtObserver.OnStart();
    }
}
