using System.Collections.Generic;
using UnityEngine;

public class LocomotionStateMachine
{
    private readonly PlayerMotionControl _motionControl;

    private readonly Dictionary<MotionStateLabel, IMotionState> _states;

    private MotionStateLabel _curentState;

    public LocomotionStateMachine(PlayerMotionControl motionControl)
    {
        _motionControl = motionControl;
        _states = new() {
            {MotionStateLabel.RUN, new RunState(_motionControl)},
            {MotionStateLabel.JUMPED, new JumpState(_motionControl)},
        };
        _curentState = MotionStateLabel.RUN;
    }

    public void SetMotionInput(Vector2 input) => _motionControl.SetMotionInput(input);

    public void OnStateAction()
    {
        if (_motionControl.Controller.enabled)
            _curentState = _states[_curentState].Action();
    }

    public void Jump()
    {
        if (!_motionControl.Controller.enabled) return;
        if (_curentState == MotionStateLabel.JUMPED) return;
        _curentState = MotionStateLabel.JUMPED;
        _motionControl.Jump();
    }
}
