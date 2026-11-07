using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting;
using Zenject;

public class SceneManager : MonoBehaviour
{
    [Inject] private readonly PlayerSpawner _playerSpawner;
    
    [Inject] private readonly EnemiesSpawner _enemiesSpawner;

    [Inject] private readonly PlayOptionsSwitch _playOptionsSwitch;

    [Inject] private readonly DamageModifiersSystem _damageModifiersSystem;

    [Inject] private readonly GameManager _gameManager;

    [Inject] private readonly TouchMode _touchMode;

    [Inject] private readonly TouchControls _touchControls;

    private UiInputActions _uiInputActions;

    private void Awake() => _uiInputActions = new();

    private void OnEnable() => setInputActionsState(enabled: true);

    private void OnDisable() => setInputActionsState(enabled: false);

    private void Start()
    {
        Cursor.visible = false;
        lockCursor(isTouch: _touchMode.IsActive);
#if UNITY_ANDROID || UNITY_IOS
        // Phones cap the game at 30 otherwise
        Application.targetFrameRate = 60;
#endif

        _damageModifiersSystem.ShowModifications();

        _playerSpawner.Spawn();
        _enemiesSpawner.GeneratePool(_playerSpawner.Player);
        _playOptionsSwitch.SetOptionState(WaveState.IDLE);
    }

    // Called by the web page (WebGLTemplates) when a phone is turned to portrait; nothing in the code refers to it
    [Preserve]
    public void PauseOnRotation()
    {
        if (!_gameManager.GamePaused) pause();
    }

    private void quit(InputAction.CallbackContext _)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // A page can't close its browser tab: quitting would only stop the player under the still running page
        // (the next frame then crashes on freed memory). The browser already gives Escape its own meaning there:
        // freeing the mouse and leaving fullscreen
#else
        Application.Quit();
#endif
    }

    private void pause(InputAction.CallbackContext _) => pause();

    private void pause() => _gameManager.SwitchPauseState();

    private void showControls(InputAction.CallbackContext _) => setControlsState(true);

    private void hideControls(InputAction.CallbackContext _) => setControlsState(false);

    private void submitAction(InputAction.CallbackContext _) => _playOptionsSwitch.ActivateOption();

    private void useTouch(InputAction.CallbackContext _) => _touchMode.OnTouch();

    private void leaveTouch(InputAction.CallbackContext _) => _touchMode.OnOtherDevice();

    // There's no pointer to capture on a touch screen, and a mobile browser would only refuse it
    private void lockCursor(bool isTouch) => Cursor.lockState = isTouch ? CursorLockMode.None : CursorLockMode.Locked;

    private void setControlsState(bool state)
    {
        if (_gameManager.GamePaused) return;
        if (state != _gameManager.ControlsVisible) _gameManager.SwitchControlsTabVisibility();
    }

    private void setInputActionsState(bool enabled)
    {
        if (enabled)
        {
            _uiInputActions.Enable();
            _uiInputActions.UI.Quit.performed += quit;
            _uiInputActions.UI.Pause.performed += pause;
            _uiInputActions.UI.Submit.performed += submitAction;
            _uiInputActions.UI.ShowControls.started += showControls;
            _uiInputActions.UI.ShowControls.canceled += hideControls;
            _uiInputActions.UI.UseTouch.performed += useTouch;
            _uiInputActions.UI.LeaveTouch.performed += leaveTouch;
            _touchControls.Pause.Pressed += pause;
            _touchControls.LookArea.ZoneTapped += pause;
            _touchMode.Changed += lockCursor;
        }
        else
        {
            _uiInputActions.Disable();
            _uiInputActions.UI.Quit.performed -= quit;
            _uiInputActions.UI.Pause.performed -= pause;
            _uiInputActions.UI.Submit.performed -= submitAction;
            _uiInputActions.UI.ShowControls.started -= showControls;
            _uiInputActions.UI.ShowControls.canceled -= hideControls;
            _uiInputActions.UI.UseTouch.performed -= useTouch;
            _uiInputActions.UI.LeaveTouch.performed -= leaveTouch;
            _touchControls.Pause.Pressed -= pause;
            _touchControls.LookArea.ZoneTapped -= pause;
            _touchMode.Changed -= lockCursor;
        }
    }
}
