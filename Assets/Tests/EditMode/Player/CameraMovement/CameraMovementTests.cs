using NUnit.Framework;
using Unity.Cinemachine;
using UnityEngine;
using Zenject;

public class CameraMovementTests : ZenjectUnitTestFixture
{
    private const float _maxSpeedX = 150;

    private const float _maxSpeedY = 100;

    private const float _turnFollowSpeed = 1;

    private const float _delta = 0.0001f;

    private GameObject _root;

    private Camera _camera;

    private CameraMovement _cameraMovement;

    [SetUp]
    public void SetUpCamera()
    {
        _root = new GameObject(nameof(CameraMovementTests));
        var target = new GameObject("Target").transform;
        target.SetParent(_root.transform);
        _camera = new GameObject("Camera").AddComponent<Camera>();
        _camera.transform.SetParent(_root.transform);

        Container.BindInstance(new CameraMovementParams
        {
            TurnFollowSpeed = _turnFollowSpeed,
            Target = target,
            CameraX = new AxisState(-180, 180, true, false, _maxSpeedX, 0, 0, "Mouse X", false),
            CameraY = new AxisState(-40, 60, false, true, _maxSpeedY, 0, 0, "Mouse Y", false),
        });
        Container.BindInstance(new CameraParams { Camera = _camera, AimTarget = target });
        Container.BindInstance(new CharacterComponents { Transform = _root.transform });
        Container.Bind<CameraMovement>().AsSingle();
        _cameraMovement = Container.Resolve<CameraMovement>();
        // An axis starts at the middle of its range
        _cameraMovement.CameraValues = Vector2.zero;
    }

    [TearDown]
    public void TearDownCamera() => Object.DestroyImmediate(_root);

    [Test]
    public void Should_Turn_By_Max_Speed_Per_Thousand_Pointer_Pixels()
    {
        _cameraMovement.RotateByPointer(new Vector2(1000, 0));

        Assert.AreEqual(_maxSpeedX, _cameraMovement.CameraValues.x, _delta, "A thousand pixels should turn by max speed");
    }

    [Test]
    public void Should_Turn_By_Pointer_Distance_Regardless_Of_Frame_Split()
    {
        for (int i = 0; i < 10; ++i) _cameraMovement.RotateByPointer(new Vector2(30, 20));
        Vector2 manyFrames = _cameraMovement.CameraValues;

        _cameraMovement.CameraValues = Vector2.zero;
        _cameraMovement.RotateByPointer(new Vector2(300, 200));
        Vector2 oneFrame = _cameraMovement.CameraValues;

        Assert.AreEqual(oneFrame.x, manyFrames.x, _delta, "Same pointer travel at a higher frame rate must turn the same");
        Assert.AreEqual(oneFrame.y, manyFrames.y, _delta, "Same pointer travel at a higher frame rate must turn the same");
    }

    [Test]
    public void Should_Turn_Slightly_On_Small_Pointer_Moves()
    {
        _cameraMovement.RotateByPointer(new Vector2(0.5f, 0));

        Assert.AreEqual(0.5f / 1000 * _maxSpeedX, _cameraMovement.CameraValues.x, _delta, "A sub-pixel move must not jump like a full stick");
    }

    [Test]
    public void Should_Turn_By_Stick_Rate_Over_Frame_Time()
    {
        _cameraMovement.RotateByStick(new Vector2(1, 0.5f));

        Assert.AreEqual(_maxSpeedX * Time.deltaTime, _cameraMovement.CameraValues.x, _delta, "A full stick should turn by max speed over the frame");
        Assert.AreEqual(0.5f * _maxSpeedY * Time.deltaTime, _cameraMovement.CameraValues.y, _delta, "A half stick should turn by half of max speed over the frame");
    }

    [Test]
    public void Should_Turn_Player_Towards_Camera_By_Frame_Time()
    {
        Quaternion cameraRotation = Quaternion.Euler(0, 90, 0);
        _camera.transform.rotation = cameraRotation;
        Quaternion expected = Quaternion.Slerp(Quaternion.identity, cameraRotation, _turnFollowSpeed * Time.deltaTime);

        _cameraMovement.RotateByPointer(Vector2.zero);

        Assert.AreEqual(0, Quaternion.Angle(expected, _root.transform.rotation), 0.01f, "The turn runs every frame, so it should go by the frame's time");
    }

    [Test]
    public void Should_Clamp_Vertical_Pointer_Turn()
    {
        _cameraMovement.RotateByPointer(new Vector2(0, 100000));

        Assert.AreEqual(60, _cameraMovement.CameraValues.y, _delta, "The vertical turn should stop at the axis limit");
    }
}
