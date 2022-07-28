using UnityEngine;

public class PlayerMotionControl
{

    public readonly MotionParams MotionParams;

    public readonly Animator Animator;

    public readonly CharacterController Controller;

    private readonly int _jumpStateHash = Animator.StringToHash("is_jumping");

    private readonly int _xLabeleHash = Animator.StringToHash("InputX");

    private readonly int _yLabeleHash = Animator.StringToHash("InputY");

    public MotionStateParams MotionState = default;

    public PlayerMotionControl(Animator animator, CharacterController controller, MotionParams motionParams)
    {
        Animator = animator;
        Controller = controller;
        MotionParams = motionParams;
    }

    public void MoveOnJump(float horizontalVelocity)
    {
        Vector3 animatorVelocity = Animator.velocity;
        MotionState.Velocity = animatorVelocity * MotionParams.JumpDamp * MotionParams.MovementSpeed;
        MotionState.Velocity.y = horizontalVelocity;
        SetJumpAnimationState(state: true);
    }

    public void SetJumpAnimationState(bool state) => Animator.SetBool(_jumpStateHash, state);

    public void MoveCharacter(Vector3 motion)
    {
        Controller.Move(motion);
        MotionState.RootMotion = Vector3.zero;
    }

    public void UpdateRootMotion() => MotionState.RootMotion += Animator.deltaPosition;

    public void SetMotionInput(Vector2 input)
    {
        MotionState.Input = Vector2.SmoothDamp(MotionState.Input, input, ref MotionState.SmoothInput, MotionParams.SmoothDampTime);
        Animator.SetFloat(_xLabeleHash, MotionState.Input.x);
        Animator.SetFloat(_yLabeleHash, MotionState.Input.y);
    }

    public void Jump()
    {
        float velocity = Mathf.Sqrt(2 * MotionParams.Gravity * MotionParams.JumpHeight);
        MoveOnJump(velocity);
    }

    public void PushObstacle(ControllerColliderHit hit)
    {
        Rigidbody attached = hit.collider.attachedRigidbody;
        if (attached == null || attached.isKinematic) return;
        var pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.x);
        attached.velocity = pushDir * MotionParams.PushForce;
    }
}
