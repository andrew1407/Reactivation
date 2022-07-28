using UnityEngine;

public class JumpState : IMotionState
{
    private readonly PlayerMotionControl _motionControl;

    public JumpState(PlayerMotionControl motionControl)
    {
        _motionControl = motionControl;
    }

    public MotionStateLabel Action()
    {
        _motionControl.MotionState.Velocity.y -= _motionControl.MotionParams.Gravity * Time.fixedDeltaTime;
        var motion = _motionControl.MotionState.Velocity * Time.fixedDeltaTime;
        _motionControl.MoveCharacter(motion + getJumpMovementPosition());
        bool jumpAction = !_motionControl.Controller.isGrounded;
        _motionControl.SetJumpAnimationState(jumpAction);
        return jumpAction ? MotionStateLabel.JUMPED : MotionStateLabel.RUN;
    }

    private Vector3 getJumpMovementPosition()
    {
        Transform transform = _motionControl.Controller.transform;
        Vector2 input = _motionControl.MotionState.Input;
        float jumpMovement = _motionControl.MotionParams.JumpMovement;
        return (transform.forward * input.y + transform.right * input.x) * jumpMovement / 100;
    }
}
