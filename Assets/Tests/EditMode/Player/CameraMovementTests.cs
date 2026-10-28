using NUnit.Framework;
using Unity.Cinemachine;
using UnityEngine;
using Zenject;

public class CameraMovementTests : ZenjectUnitTestFixture
{
    private const float MaxSpeedX = 150;

    private const float MaxSpeedY = 100;

    private const float Delta = 0.0001f;

    private GameObject _root;

    private CameraMovement _cameraMovement;

    [SetUp]
    public void SetUpCamera()
    {
        _root = new GameObject(nameof(CameraMovementTests));
        var target = new GameObject("Target").transform;
        target.SetParent(_root.transform);
        var camera = new GameObject("Camera").AddComponent<Camera>();
        camera.transform.SetParent(_root.transform);

        Container.BindInstance(new CameraMovementParams
        {
            TurnFollowSpeed = 1,
            Target = target,
            CameraX = new AxisState(-180, 180, true, false, MaxSpeedX, 0, 0, "Mouse X", false),
            CameraY = new AxisState(-40, 60, false, true, MaxSpeedY, 0, 0, "Mouse Y", false),
        });
        Container.BindInstance(new CameraParams { Camera = camera, AimTarget = target });
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

        Assert.AreEqual(MaxSpeedX, _cameraMovement.CameraValues.x, Delta);
    }

    [Test]
    public void Should_Turn_By_Pointer_Distance_Regardless_Of_Frame_Split()
    {
        for (int i = 0; i < 10; ++i) _cameraMovement.RotateByPointer(new Vector2(30, 20));
        Vector2 manyFrames = _cameraMovement.CameraValues;

        _cameraMovement.CameraValues = Vector2.zero;
        _cameraMovement.RotateByPointer(new Vector2(300, 200));
        Vector2 oneFrame = _cameraMovement.CameraValues;

        Assert.AreEqual(oneFrame.x, manyFrames.x, Delta, "Same pointer travel at a higher frame rate must turn the same");
        Assert.AreEqual(oneFrame.y, manyFrames.y, Delta);
    }

    [Test]
    public void Should_Turn_Slightly_On_Small_Pointer_Moves()
    {
        _cameraMovement.RotateByPointer(new Vector2(0.5f, 0));

        Assert.AreEqual(0.5f / 1000 * MaxSpeedX, _cameraMovement.CameraValues.x, Delta, "A sub-pixel move must not jump like a full stick");
    }

    [Test]
    public void Should_Turn_By_Stick_Rate_Over_Frame_Time()
    {
        _cameraMovement.RotateByStick(new Vector2(1, 0.5f));

        Assert.AreEqual(MaxSpeedX * Time.deltaTime, _cameraMovement.CameraValues.x, Delta);
        Assert.AreEqual(0.5f * MaxSpeedY * Time.deltaTime, _cameraMovement.CameraValues.y, Delta);
    }

    [Test]
    public void Should_Clamp_Vertical_Pointer_Turn()
    {
        _cameraMovement.RotateByPointer(new Vector2(0, 100000));

        Assert.AreEqual(60, _cameraMovement.CameraValues.y, Delta);
    }
}
