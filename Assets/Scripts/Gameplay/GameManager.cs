using UnityEngine;
using Zenject;

public class GameManager
{
    [Inject] private readonly InfoMessageViewer _infoMessageViewer;

    public GameManager()
    {
        GamePaused = false;
        ControlsVisible = false;
    }

    public bool GamePaused { get; private set; }

    public bool ControlsVisible { get; private set; }

    public void SwitchPauseState()
    {
        _infoMessageViewer.PauseTabActive = GamePaused = !GamePaused;
        if (_infoMessageViewer.ControlsTabActive) SwitchControlsTabVisibility();
        Time.timeScale = GamePaused ? 0 : 1;
    }

    public void SwitchControlsTabVisibility()
    {
        _infoMessageViewer.ControlsTabActive = ControlsVisible = !ControlsVisible;
        Time.timeScale = ControlsVisible ? 0.25f : 1;
    }
}
