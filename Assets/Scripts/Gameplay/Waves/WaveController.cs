using Zenject;

public class WavesController: ITickable
{
    [Inject] private readonly UiController _uiController;

    [Inject] private readonly PlayerActivityObserver _activityObserver;

    [Inject] private readonly Timer _timer;

    [Inject] private readonly uint _wavesCount;
    
    private uint _currentWave = 1;

    public bool FinalWave { get => _currentWave == _wavesCount; }

    public void Tick()
    {
        if (!_timer.Active) return;
        _timer.OnTick();
        _uiController.Timer = _timer.CurrentTime;
        if (_timer.CurrentTime == 0)
            _activityObserver.OnWaveEnd(WaveState.TIME_IS_OVER);
    }

    public void ResetParameters()
    {
        _timer.Reset();
        _uiController.Timer = _timer.CurrentTime;
        _uiController.Wave = _currentWave = 1;
    }

    public void SetNextWave() => _uiController.Wave = ++_currentWave;
}
