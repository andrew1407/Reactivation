using UnityEngine;

public class RunState : IMotionState
{
    private readonly PlayerMotionControl _motionControl;

    public RunState(PlayerMotionControl motionControl)
    {
        _motionControl = motionControl;
    }

    public MotionStateLabel Action()
    {
        var falling = Vector3.down * _motionControl.MotionParams.StepDown;
        var motion = _motionControl.MotionState.RootMotion * _motionControl.MotionParams.MovementSpeed + falling;
        _motionControl.MoveCharacter(motion);
        if (_motionControl.Controller.isGrounded) return MotionStateLabel.RUN;
        _motionControl.MoveOnJump(horizontalVelocity: 0);
        return MotionStateLabel.JUMPED;
    }
}
