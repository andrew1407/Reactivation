using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

// The bindings that move the game on and off the touch controls, on simulated devices
public class TouchSwitchActionsTests : InputTestFixture
{
    private UiInputActions _actions;

    private int _touchUses;

    private int _touchLeaves;

    public override void Setup()
    {
        base.Setup();
        _actions = new UiInputActions();
        _touchUses = _touchLeaves = 0;
        _actions.UI.UseTouch.performed += _ => ++_touchUses;
        _actions.UI.LeaveTouch.performed += _ => ++_touchLeaves;
        _actions.Enable();
    }

    public override void TearDown()
    {
        // The generated Dispose() destroys the asset in a way edit mode refuses
        _actions.Disable();
        Object.DestroyImmediate(_actions.asset);
        base.TearDown();
    }

    [Test]
    public void Should_Leave_Touch_On_Gamepad_Button()
    {
        var gamepad = InputSystem.AddDevice<Gamepad>();

        Press(gamepad.buttonSouth);

        Assert.AreEqual(1, _touchLeaves, "A gamepad button should leave touch");
    }

    [Test]
    public void Should_Leave_Touch_On_Gamepad_Stick()
    {
        var gamepad = InputSystem.AddDevice<Gamepad>();

        Set(gamepad.leftStick, new Vector2(1, 0));

        Assert.AreEqual(1, _touchLeaves, "A tilted gamepad stick should leave touch");
    }

    [Test]
    public void Should_Not_Leave_Touch_On_Resting_Stick_Noise()
    {
        var gamepad = InputSystem.AddDevice<Gamepad>();

        Set(gamepad.leftStick, new Vector2(0.05f, 0.02f));

        Assert.AreEqual(0, _touchLeaves, "A resting stick shouldn't leave touch");
    }

    [Test]
    public void Should_Leave_Touch_On_Keyboard()
    {
        var keyboard = InputSystem.AddDevice<Keyboard>();

        Press(keyboard.wKey);

        Assert.AreEqual(1, _touchLeaves, "A key should leave touch");
    }

    [Test]
    public void Should_Use_Touch_On_Touch()
    {
        InputSystem.AddDevice<Touchscreen>();

        BeginTouch(1, new Vector2(100, 100));

        Assert.AreEqual(1, _touchUses, "A touch should use touch");
        Assert.AreEqual(0, _touchLeaves, "A touch shouldn't leave touch");
    }
}
