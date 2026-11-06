using NUnit.Framework;
using UnityEngine;

public class TouchUtilsTests
{
    private const float _delta = 0.0001f;

    [Test]
    public void Should_Count_Pinch_Both_Ways_From_Its_Distance()
    {
        Assert.IsTrue(TouchUtils.IsPinch(gapChange: -80, pinchDistance: 80), "Fingers coming together should count");
        Assert.IsTrue(TouchUtils.IsPinch(gapChange: 100, pinchDistance: 80), "Fingers moving apart should count");
        Assert.IsFalse(TouchUtils.IsPinch(gapChange: 79, pinchDistance: 80), "A shorter change shouldn't count");
    }

    [Test]
    public void Should_Start_Pinch_Only_On_Fingers_Moving_Against_Each_Other()
    {
        Assert.IsTrue(TouchUtils.IsPinchStart(new Vector2(20, 0), new Vector2(-20, 0), minTravel: 15), "Opposite travel should start");
        Assert.IsFalse(TouchUtils.IsPinchStart(new Vector2(200, 0), Vector2.zero, minTravel: 15), "A resting finger shouldn't start");
        Assert.IsFalse(TouchUtils.IsPinchStart(new Vector2(20, 0), new Vector2(20, 0), minTravel: 15), "Fingers swiping alongside shouldn't start");
        Assert.IsFalse(TouchUtils.IsPinchStart(new Vector2(14, 0), new Vector2(-20, 0), minTravel: 15), "A shorter travel shouldn't start");
    }

    [Test]
    public void Should_Tell_When_A_Button_Reaches_Stick_Zone()
    {
        Assert.IsFalse(TouchUtils.IsOverStickZone(width: 1920, edgeFromRight: 879, stickZoneShare: 0.4f), "A wide screen should have room");
        Assert.IsTrue(TouchUtils.IsOverStickZone(width: 1440, edgeFromRight: 879, stickZoneShare: 0.4f), "A 4:3 screen shouldn't");
    }

    [Test]
    public void Should_Scale_Stick_By_Its_Radius()
    {
        Vector2 value = TouchUtils.ToStickValue(new Vector2(30, -15), radius: 60);

        Assert.AreEqual(0.5f, value.x, _delta, "X should be a share of the radius");
        Assert.AreEqual(-0.25f, value.y, _delta, "Y should be a share of the radius");
    }

    [Test]
    public void Should_Limit_Stick_To_Full_Tilt()
    {
        Vector2 value = TouchUtils.ToStickValue(new Vector2(300, 400), radius: 50);

        Assert.AreEqual(1, value.magnitude, _delta, "The tilt shouldn't go over full");
        Assert.AreEqual(0.6f, value.x, _delta, "The direction should be kept");
    }

    // The same directions the gamepad's cross reports, so the sides pick the slots it picks
    [TestCase(10, 100, 0, 1)]
    [TestCase(-10, -100, 0, -1)]
    [TestCase(-100, 10, -1, 0)]
    [TestCase(100, -10, 1, 0)]
    public void Should_Pick_Slot_By_Dominant_Axis(float x, float y, float slotX, float slotY)
    {
        Assert.AreEqual(new Vector2(slotX, slotY), TouchUtils.ToWeaponSlot(new Vector2(x, y)), "The longer axis should pick the slot");
    }

    [Test]
    public void Should_Leave_Ring_Corners_Free()
    {
        Assert.IsTrue(TouchUtils.IsOnRing(new Vector2(300, 300), radius: 450), "A point within the radius is on the ring");
        Assert.IsFalse(TouchUtils.IsOnRing(new Vector2(400, 400), radius: 450), "A corner of its rectangle isn't on the ring");
    }

    [Test]
    public void Should_Take_Long_Travel_As_Swipe()
    {
        Assert.IsTrue(TouchUtils.IsSwipe(new Vector2(30, 40), swipeDistance: 50), "Travel of the swipe distance is a swipe");
        Assert.IsFalse(TouchUtils.IsSwipe(new Vector2(10, 10), swipeDistance: 50), "Shorter travel isn't a swipe");
    }
}
