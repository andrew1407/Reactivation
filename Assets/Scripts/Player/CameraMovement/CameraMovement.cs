using UnityEngine;
using Cinemachine;
using Zenject;

public class CameraMovement
{
    [Inject] private CameraMovementParams _movementParams;

    [Inject] private readonly CharacterComponents _characterComponents;

    private const float _mouseMoveDelta = 0.05f;

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

    public void UpdateRotation(Vector2 direction)
    {
        if (direction.magnitude > 1) direction *= _mouseMoveDelta;
        CameraValues += direction * Time.deltaTime * _rotationParamsState;
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
