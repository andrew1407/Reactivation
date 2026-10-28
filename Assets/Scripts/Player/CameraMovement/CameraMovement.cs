using UnityEngine;
using Unity.Cinemachine;
using Zenject;

public class CameraMovement
{
    [Inject] private CameraMovementParams _movementParams;

    [Inject] private readonly CharacterComponents _characterComponents;

    // Pointer travel (pixels) that turns the camera as far as a full stick does in one second
    private const float _pointerPixelsPerMaxSpeed = 1000;

    private readonly int _aimingLabelHash = Animator.StringToHash("aiming");

    private Vector2 _rotationParamsState;

    private Transform _cameraTransform;

    public bool Aiming
    {
        get => _characterComponents.BodyAnimator.GetBool(_aimingLabelHash);
        set => _characterComponents.BodyAnimator.SetBool(_aimingLabelHash, value);
    }

    public Vector2 CameraValues
    {
        get => new(_movementParams.CameraX.Value, _movementParams.CameraY.Value);
        set
        {
            _movementParams.CameraX.Value = GameUtils.RoundClampAxisValue(ref _movementParams.CameraX, value.x);
            _movementParams.CameraY.Value = GameUtils.ClampAxisValue(ref _movementParams.CameraY, value.y);
        }
    }

    [Inject]
    private void Construct(CameraParams cameraParams)
    {
        _cameraTransform = cameraParams.Camera.transform;
        var rotationMaxSpeed = new Vector2(_movementParams.CameraX.m_MaxSpeed, _movementParams.CameraY.m_MaxSpeed);
        var rotationInversion = new Vector2(getInversedValue(ref _movementParams.CameraX), getInversedValue(ref _movementParams.CameraY));
        _rotationParamsState = rotationMaxSpeed * rotationInversion;
    }

    // A stick reports a rate (-1..1), so it turns by max speed over the frame's time
    public void RotateByStick(Vector2 direction) => rotate(direction * Time.deltaTime * _rotationParamsState);

    // A pointer reports the distance moved since the last frame: it maps straight to an angle,
    // with no frame time involved, so the turn doesn't depend on frame rate (or window size)
    public void RotateByPointer(Vector2 delta) => rotate(delta / _pointerPixelsPerMaxSpeed * _rotationParamsState);

    private void rotate(Vector2 angles)
    {
        CameraValues += angles;
        _movementParams.Target.eulerAngles = new Vector3(_movementParams.CameraY.Value, _movementParams.CameraX.Value, 0);
        rotatePlayerByCamera();
    }

    private int getInversedValue(ref AxisState axisState) => axisState.m_InvertInput ? -1 : 1;

    private void rotatePlayerByCamera()
    {
        float y = _cameraTransform.rotation.eulerAngles.y;
        float ratio = _movementParams.TurnFollowSpeed * Time.fixedDeltaTime;
        var transform = _characterComponents.Transform;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, y, 0), ratio);
    }
}
