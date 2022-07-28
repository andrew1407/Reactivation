using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class SceneManager : MonoBehaviour
{
    [Inject] private readonly PlayerSpawner _playerSpawner;
    
    [Inject] private readonly EnemiesSpawner _enemiesSpawner;

    [Inject] private readonly PlayOptionsSwitch _playOptionsSwitch;

    [Inject] private readonly DamageModifiersSystem _damageModifiersSystem;

    [Inject] private readonly GameManager _gameManager;

    private UiInputActions _uiInputActions;

    private void Awake() => _uiInputActions = new();

    private void OnEnable() => setInputActionsState(enabled: true);

    private void OnDisable() => setInputActionsState(enabled: false);

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        _damageModifiersSystem.ShowModifications();

        _playerSpawner.Spawn();
        _enemiesSpawner.GeneratePool(_playerSpawner.Player);
        _playOptionsSwitch.SetOptionState(WaveState.IDLE);
    }

    private void quit(InputAction.CallbackContext _) => Application.Quit();

    private void pause(InputAction.CallbackContext _) => _gameManager.SwitchPauseState();

    private void showControls(InputAction.CallbackContext _) => setControlsState(true);

    private void hideControls(InputAction.CallbackContext _) => setControlsState(false);

    private void submitAction(InputAction.CallbackContext _) => _playOptionsSwitch.ActivateOption();

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
        }
        else
        {
            _uiInputActions.Disable();
            _uiInputActions.UI.Quit.performed -= quit;
            _uiInputActions.UI.Pause.performed -= pause;
            _uiInputActions.UI.Submit.performed -= submitAction;
            _uiInputActions.UI.ShowControls.started -= showControls;
            _uiInputActions.UI.ShowControls.canceled -= hideControls;
        }
    }
}
