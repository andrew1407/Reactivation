using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class PlayerInputController : CharacterAim
{
    #region Variables

    [Inject] private readonly GameManager _gameManager;

    [Inject] private readonly CameraParams _cameraParams;

    [Inject] private readonly CameraMovement _cameraMovement;

    [Inject] private readonly LocomotionStateMachine _locomotionStateMachine;

    [Inject] private PlayerWeaponSelector _weaponSelector;

    [Inject] private IWeapon[] _weapons;

    private readonly Dictionary<Vector2, Action> _weaponSlots = new();

    private PlayerInputActions _inputActions;

    private IEnumerator _currentAction;

    private bool _attacking = false;

    #endregion

    #region MonoBehaviourHandlers
    
    private void Awake()
    {
        _inputActions = new();
        Func<int, Action> selectWeapon = i => () =>
        {
            if (_currentAction == null) startAction(_weaponSelector.SwitchWeapon(i));
        };
        _weaponSlots[new(0, -1)] = selectWeapon(0);
        _weaponSlots[new(0, 1)] = selectWeapon(1);
        _weaponSlots[new(-1, 0)] = selectWeapon(2);
        _weaponSlots[new(1, 0)] = selectWeapon(3);
    }

    private void Start()
    {
        SetAim(_cameraParams.AimTarget);
        foreach (var weapon in _weapons)
            if (weapon is IAmmoContainer ammoContainer) ammoContainer.Setup();    
    }

    private void OnEnable() => setInputActionsState(enabled: true);

    private void OnDisable()
    {
        setInputActionsState(enabled: false);
        _attacking = false;
        _currentAction = null;
    }

    private void Update()
    {
        if (_gameManager.GamePaused) return;
        updateLocomotionState();
        updateCameraRotation();
        if (_attacking && _currentAction == null) startAction(useWeapon());
    }

    private void FixedUpdate()
    {
        if (_gameManager.GamePaused) return;
        _locomotionStateMachine.OnStateAction();
    }

    #endregion

    #region InputContextHandlers

    private void turnOnAiming(InputAction.CallbackContext _) => _cameraMovement.Aiming = true;

    private void turnOffAiming(InputAction.CallbackContext _) => _cameraMovement.Aiming = false;

    private void enableAttack(InputAction.CallbackContext _) => _attacking = true;

    private void disableAttack(InputAction.CallbackContext _) => _attacking = false;

    private void switchWeapon(InputAction.CallbackContext context)
    {
        var key = context.ReadValue<Vector2>();
        if (_weaponSlots.ContainsKey(key)) _weaponSlots[key]();
    }

    private void scrollWeapons(InputAction.CallbackContext context)
    {
        if (_currentAction != null) return;
        int offset = Math.Sign(context.ReadValue<float>());
        startAction(offset == 1 ? _weaponSelector.SetNext() : _weaponSelector.SetPrevious());
    }

    #endregion

    #region ToolingMethods

    private void updateCameraRotation()
    {
        var lookDirection = _inputActions.Player.Look.ReadValue<Vector2>();
        _cameraMovement.UpdateRotation(lookDirection);
    }

    private void updateLocomotionState()
    {
        var input = _inputActions.Player.Move.ReadValue<Vector2>();
        _locomotionStateMachine.SetMotionInput(input);
    }

    private void jump(InputAction.CallbackContext _)
    {
        if (!_gameManager.GamePaused) _locomotionStateMachine.Jump();
    }

    private IEnumerator waitForActionFinish(IEnumerator action)
    {
        _currentAction = action;
        yield return action;
        _currentAction = null;
    }

    private void startAction(IEnumerator action) => StartCoroutine(waitForActionFinish(action));

    private IEnumerator useWeapon()
    {
        yield return _weaponSelector.SelectedWeapon.Action();
        if (_weaponSelector.SelectedWeapon is IAmmoContainer ammoContainer)
            StartCoroutine(ammoContainer.Reload());
    }

    private void setInputActionsState(bool enabled)
    {
        if (enabled)
        {
            _inputActions.Enable();
            _inputActions.Player.Jump.started += jump;
            _inputActions.Player.Aim.started += turnOnAiming;
            _inputActions.Player.Aim.canceled += turnOffAiming;
            _inputActions.Player.Attack.started += enableAttack;
            _inputActions.Player.Attack.canceled += disableAttack;
            _inputActions.Player.WeaponSlots.started += switchWeapon;
            _inputActions.Player.WeaponSelector.started += scrollWeapons;
        }
        else
        {
            _inputActions.Disable();
            _inputActions.Player.Jump.started -= jump;
            _inputActions.Player.Aim.started -= turnOnAiming;
            _inputActions.Player.Aim.canceled -= turnOffAiming;
            _inputActions.Player.Attack.started -= enableAttack;
            _inputActions.Player.Attack.canceled -= disableAttack;
            _inputActions.Player.WeaponSlots.started -= switchWeapon;
            _inputActions.Player.WeaponSelector.started -= scrollWeapons;
        }
    }
    
    #endregion
}
