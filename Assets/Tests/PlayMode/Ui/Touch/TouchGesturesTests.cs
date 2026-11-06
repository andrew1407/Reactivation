using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

// Drives the on-screen controls through the handlers the event system calls. In play mode so that
// Awake and OnDisable run, which is where a finger lifted out of sight gets forgotten
public class TouchGesturesTests
{
    private const int _firstFinger = 1;

    private const int _secondFinger = 2;

    private const int _thirdFinger = 3;

    private const float _delta = 0.001f;

    private GameObject _root;

    private Canvas _canvas;

    [SetUp]
    public void SetUpCanvas()
    {
        _root = new GameObject(nameof(TouchGesturesTests));
        _canvas = _root.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
    }

    [TearDown]
    public void TearDownCanvas() => Object.DestroyImmediate(_root);

    #region Stick

    [Test]
    public void Should_Put_Stick_Under_Pressing_Finger()
    {
        (TouchStick stick, RectTransform view, _) = makeStick();

        stick.OnPointerDown(makeEvent(_firstFinger, screenPoint(stick, new Vector2(40, -20))));

        Assert.IsTrue(view.gameObject.activeSelf, "The stick should show while held");
        Assert.AreEqual(40, view.anchoredPosition.x, _delta, "The stick should appear under the finger");
        Assert.AreEqual(-20, view.anchoredPosition.y, _delta, "The stick should appear under the finger");
    }

    [Test]
    public void Should_Tilt_Stick_By_Drag_And_Move_Knob()
    {
        (TouchStick stick, _, RectTransform knob) = makeStick();
        stick.OnPointerDown(makeEvent(_firstFinger, screenPoint(stick, Vector2.zero)));

        stick.OnDrag(makeEvent(_firstFinger, screenPoint(stick, new Vector2(37.5f, 0))));

        Assert.AreEqual(0.5f, stick.Value.x, _delta, "Half the radius should be half a tilt");
        Assert.AreEqual(37.5f, knob.anchoredPosition.x, _delta, "The knob should follow the finger");
    }

    [Test]
    public void Should_Follow_Only_First_Finger_On_Stick()
    {
        (TouchStick stick, RectTransform view, _) = makeStick();
        stick.OnPointerDown(makeEvent(_firstFinger, screenPoint(stick, Vector2.zero)));

        stick.OnPointerDown(makeEvent(_secondFinger, screenPoint(stick, new Vector2(50, 50))));
        stick.OnDrag(makeEvent(_secondFinger, screenPoint(stick, new Vector2(70, 0))));
        Assert.AreEqual(Vector2.zero, stick.Value, "A second finger shouldn't tilt the stick");
        Assert.AreEqual(Vector2.zero, view.anchoredPosition, "A second finger shouldn't move the stick");

        stick.OnDrag(makeEvent(_firstFinger, screenPoint(stick, new Vector2(75, 0))));
        stick.OnPointerUp(makeEvent(_secondFinger, Vector2.zero));
        Assert.AreEqual(1, stick.Value.x, _delta, "Lifting a second finger shouldn't release the stick");
        Assert.IsTrue(view.gameObject.activeSelf, "Lifting a second finger shouldn't hide the stick");
    }

    [Test]
    public void Should_Reset_Stick_On_Release()
    {
        (TouchStick stick, RectTransform view, RectTransform knob) = makeStick();
        stick.OnPointerDown(makeEvent(_firstFinger, screenPoint(stick, Vector2.zero)));
        stick.OnDrag(makeEvent(_firstFinger, screenPoint(stick, new Vector2(75, 0))));

        stick.OnPointerUp(makeEvent(_firstFinger, Vector2.zero));

        Assert.AreEqual(Vector2.zero, stick.Value, "A released stick should center");
        Assert.AreEqual(Vector2.zero, knob.anchoredPosition, "A released knob should center");
        Assert.IsFalse(view.gameObject.activeSelf, "A released stick should hide");
    }

    [Test]
    public void Should_Release_Stick_When_Hidden_Under_Finger()
    {
        (TouchStick stick, RectTransform view, _) = makeStick();
        stick.OnPointerDown(makeEvent(_firstFinger, screenPoint(stick, Vector2.zero)));
        stick.OnDrag(makeEvent(_firstFinger, screenPoint(stick, new Vector2(75, 0))));

        stick.gameObject.SetActive(false);
        stick.gameObject.SetActive(true);

        Assert.AreEqual(Vector2.zero, stick.Value, "A hidden stick should center");
        Assert.IsFalse(view.gameObject.activeSelf, "A hidden stick shouldn't come back shown");

        stick.OnPointerDown(makeEvent(_secondFinger, screenPoint(stick, Vector2.zero)));
        Assert.IsTrue(view.gameObject.activeSelf, "The lost finger shouldn't keep others from the stick");
    }

    [Test]
    public void Should_Hide_Stick_Disabled_Without_A_Finger()
    {
        (TouchStick stick, RectTransform view, _) = makeStick();

        stick.gameObject.SetActive(false);

        Assert.IsFalse(view.gameObject.activeSelf, "Disabling should leave the stick hidden even if nothing held it");
    }

    #endregion

    #region LookArea

    [Test]
    public void Should_Sum_Look_Drags_In_Canvas_Units_Until_Consumed()
    {
        _canvas.scaleFactor = 2;
        TouchLookArea lookArea = makeControl<TouchLookArea>();
        lookArea.OnPointerDown(makeEvent(_firstFinger, Vector2.zero));

        lookArea.OnDrag(makeEvent(_firstFinger, Vector2.zero, new Vector2(10, 4)));
        lookArea.OnDrag(makeEvent(_firstFinger, Vector2.zero, new Vector2(10, 0)));

        Assert.AreEqual(new Vector2(10, 2), lookArea.ConsumeDelta(), "Drags should add up, divided by the canvas scale");
        Assert.AreEqual(Vector2.zero, lookArea.ConsumeDelta(), "Consumed travel shouldn't count twice");
    }

    [Test]
    public void Should_Follow_Only_First_Finger_On_Look_Area()
    {
        TouchLookArea lookArea = makeControl<TouchLookArea>();
        lookArea.OnPointerDown(makeEvent(_firstFinger, Vector2.zero));
        lookArea.OnPointerDown(makeEvent(_secondFinger, Vector2.zero));

        lookArea.OnDrag(makeEvent(_secondFinger, Vector2.zero, new Vector2(10, 0)));
        Assert.AreEqual(Vector2.zero, lookArea.ConsumeDelta(), "A second finger shouldn't turn the camera");

        lookArea.OnPointerUp(makeEvent(_secondFinger, Vector2.zero));
        lookArea.OnDrag(makeEvent(_firstFinger, Vector2.zero, new Vector2(10, 0)));
        Assert.AreEqual(new Vector2(10, 0), lookArea.ConsumeDelta(), "Lifting a second finger shouldn't stop the first");
    }

    [Test]
    public void Should_Ignore_Look_Drags_After_Release()
    {
        TouchLookArea lookArea = makeControl<TouchLookArea>();
        lookArea.OnPointerDown(makeEvent(_firstFinger, Vector2.zero));

        lookArea.OnPointerUp(makeEvent(_firstFinger, Vector2.zero));
        lookArea.OnDrag(makeEvent(_firstFinger, Vector2.zero, new Vector2(10, 0)));

        Assert.AreEqual(Vector2.zero, lookArea.ConsumeDelta(), "A lifted finger shouldn't turn the camera");
    }

    [Test]
    public void Should_Drop_Look_Travel_When_Hidden()
    {
        TouchLookArea lookArea = makeControl<TouchLookArea>();
        lookArea.OnPointerDown(makeEvent(_firstFinger, Vector2.zero));
        lookArea.OnDrag(makeEvent(_firstFinger, Vector2.zero, new Vector2(10, 0)));

        lookArea.gameObject.SetActive(false);
        lookArea.gameObject.SetActive(true);
        Assert.AreEqual(Vector2.zero, lookArea.ConsumeDelta(), "Travel from before hiding shouldn't turn the camera");

        lookArea.OnPointerDown(makeEvent(_secondFinger, Vector2.zero));
        lookArea.OnDrag(makeEvent(_secondFinger, Vector2.zero, new Vector2(4, 0)));
        Assert.AreEqual(new Vector2(4, 0), lookArea.ConsumeDelta(), "The lost finger shouldn't keep others from the area");
    }

    [Test]
    public void Should_Report_Fingers_Coming_Together_And_Moving_Apart()
    {
        (TouchLookArea lookArea, List<bool> pinches) = makePinchedLookArea();

        moveFingers(lookArea, 50, 350);
        moveFingers(lookArea, 0, 400);

        CollectionAssert.AreEqual(new[] { true, false }, pinches, "Each long enough change of the gap should be reported");
    }

    [Test]
    public void Should_Measure_Pinch_In_Canvas_Units()
    {
        _canvas.scaleFactor = 2;
        (TouchLookArea lookArea, List<bool> pinches) = makePinchedLookArea();

        moveFingers(lookArea, 50, 300);
        CollectionAssert.IsEmpty(pinches, "Under the distance once divided by the canvas scale");

        moveFingers(lookArea, 60, 300);
        CollectionAssert.AreEqual(new[] { true }, pinches, "Over the distance once divided by the canvas scale");
    }

    [Test]
    public void Should_Count_Turning_Back_From_Farthest_Point_Of_Pinch()
    {
        (TouchLookArea lookArea, List<bool> pinches) = makePinchedLookArea();

        moveFingers(lookArea, 35, 365);
        moveFingers(lookArea, 0, 410);

        CollectionAssert.AreEqual(new[] { false }, pinches, "Moving apart should count from the smallest gap reached");
    }

    [Test]
    public void Should_Not_Turn_Camera_While_Pinching()
    {
        (TouchLookArea lookArea, _) = makePinchedLookArea();
        moveFingers(lookArea, 20, 380);

        lookArea.OnDrag(makeEvent(_firstFinger, new Vector2(20, 0), new Vector2(10, 0)));
        Assert.AreEqual(Vector2.zero, lookArea.ConsumeDelta(), "A pinching finger shouldn't turn the camera");

        lookArea.OnPointerUp(makeEvent(_secondFinger, Vector2.zero));
        lookArea.OnDrag(makeEvent(_firstFinger, new Vector2(20, 0), new Vector2(10, 0)));
        Assert.AreEqual(new Vector2(10, 0), lookArea.ConsumeDelta(), "The finger left should turn the camera again");
    }

    [Test]
    public void Should_Keep_Turning_Camera_With_A_Resting_Second_Finger()
    {
        (TouchLookArea lookArea, List<bool> pinches) = makePinchedLookArea();

        lookArea.OnDrag(makeEvent(_firstFinger, new Vector2(300, 0), new Vector2(300, 0)));

        Assert.AreEqual(new Vector2(300, 0), lookArea.ConsumeDelta(), "A resting finger shouldn't stop the camera");
        CollectionAssert.IsEmpty(pinches, "A resting finger isn't a pinch");
    }

    [Test]
    public void Should_Turn_Camera_With_Finger_Left_When_First_One_Lifts()
    {
        (TouchLookArea lookArea, List<bool> pinches) = makePinchedLookArea();
        moveFingers(lookArea, 20, 380);

        lookArea.OnPointerUp(makeEvent(_firstFinger, Vector2.zero));
        lookArea.OnDrag(makeEvent(_secondFinger, new Vector2(300, 0), new Vector2(10, 0)));

        Assert.AreEqual(new Vector2(10, 0), lookArea.ConsumeDelta(), "The finger left should take over the camera");
        CollectionAssert.IsEmpty(pinches, "One finger left isn't a pinch");
    }

    [Test]
    public void Should_Pinch_With_Third_Finger_When_Second_One_Lifts()
    {
        (TouchLookArea lookArea, List<bool> pinches) = makePinchedLookArea();
        lookArea.OnPointerDown(makeEvent(_thirdFinger, new Vector2(400, 0)));

        lookArea.OnPointerUp(makeEvent(_secondFinger, Vector2.zero));
        lookArea.OnDrag(makeEvent(_firstFinger, new Vector2(50, 0)));
        lookArea.OnDrag(makeEvent(_thirdFinger, new Vector2(350, 0)));

        CollectionAssert.AreEqual(new[] { true }, pinches, "The third finger should take the second's place");
    }

    [Test]
    public void Should_Forget_Pinch_When_Hidden()
    {
        (TouchLookArea lookArea, List<bool> pinches) = makePinchedLookArea();

        lookArea.gameObject.SetActive(false);
        lookArea.gameObject.SetActive(true);
        lookArea.OnDrag(makeEvent(_secondFinger, Vector2.zero));

        CollectionAssert.IsEmpty(pinches, "A pinch cut off by hiding shouldn't report anything");
    }

    [Test]
    public void Should_Report_Tap_In_Tap_Zone_Only()
    {
        (TouchLookArea lookArea, List<int> taps) = makeLookAreaWithTapZone();

        lookArea.OnPointerDown(makeEvent(_firstFinger, screenPoint(lookArea, new Vector2(100, 0))));
        lookArea.OnPointerUp(makeEvent(_firstFinger, Vector2.zero));
        Assert.AreEqual(0, taps.Count, "A tap outside the zone shouldn't count");

        lookArea.OnPointerDown(makeEvent(_firstFinger, screenPoint(lookArea, Vector2.zero)));
        lookArea.OnPointerUp(makeEvent(_firstFinger, Vector2.zero));
        Assert.AreEqual(1, taps.Count, "A tap in the zone should count");
    }

    [Test]
    public void Should_Turn_Camera_And_Not_Tap_On_Drag_From_Tap_Zone()
    {
        (TouchLookArea lookArea, List<int> taps) = makeLookAreaWithTapZone();
        Vector2 press = screenPoint(lookArea, Vector2.zero);
        lookArea.OnPointerDown(makeEvent(_firstFinger, press));

        lookArea.OnDrag(makeEvent(_firstFinger, press + new Vector2(30, 0), new Vector2(30, 0)));
        lookArea.OnDrag(makeEvent(_firstFinger, press, new Vector2(-30, 0)));
        lookArea.OnPointerUp(makeEvent(_firstFinger, press));

        Assert.AreEqual(Vector2.zero, lookArea.ConsumeDelta(), "The drag should have turned the camera there and back");
        Assert.AreEqual(0, taps.Count, "A drag isn't a tap, even when it comes back");
    }

    [Test]
    public void Should_Report_Tap_While_Another_Finger_Turns_Camera()
    {
        (TouchLookArea lookArea, List<int> taps) = makeLookAreaWithTapZone();
        Vector2 zone = screenPoint(lookArea, Vector2.zero);
        lookArea.OnPointerDown(makeEvent(_firstFinger, screenPoint(lookArea, new Vector2(100, 0))));

        lookArea.OnPointerDown(makeEvent(_secondFinger, zone));
        lookArea.OnPointerUp(makeEvent(_secondFinger, zone));
        Assert.AreEqual(1, taps.Count, "A finger already looking shouldn't stop another from tapping");

        lookArea.OnDrag(makeEvent(_firstFinger, screenPoint(lookArea, new Vector2(110, 0)), new Vector2(10, 0)));
        Assert.AreEqual(new Vector2(10, 0), lookArea.ConsumeDelta(), "The first finger should keep turning the camera");
    }

    [Test]
    public void Should_Report_Tap_Of_Finger_Left_When_First_One_Lifts()
    {
        (TouchLookArea lookArea, List<int> taps) = makeLookAreaWithTapZone();
        Vector2 zone = screenPoint(lookArea, Vector2.zero);
        lookArea.OnPointerDown(makeEvent(_firstFinger, screenPoint(lookArea, new Vector2(100, 0))));
        lookArea.OnPointerDown(makeEvent(_secondFinger, zone));

        lookArea.OnPointerUp(makeEvent(_firstFinger, Vector2.zero));
        Assert.AreEqual(0, taps.Count, "The tapping finger hasn't lifted yet");

        lookArea.OnPointerUp(makeEvent(_secondFinger, zone));
        Assert.AreEqual(1, taps.Count, "The tap should survive the other finger lifting");
    }

    [Test]
    public void Should_Not_Tap_On_Second_Finger_Dragging_From_Tap_Zone()
    {
        (TouchLookArea lookArea, List<int> taps) = makeLookAreaWithTapZone();
        Vector2 zone = screenPoint(lookArea, Vector2.zero);
        lookArea.OnPointerDown(makeEvent(_firstFinger, screenPoint(lookArea, new Vector2(100, 0))));
        lookArea.OnPointerDown(makeEvent(_secondFinger, zone));

        lookArea.OnDrag(makeEvent(_secondFinger, zone + new Vector2(30, 0)));
        lookArea.OnPointerUp(makeEvent(_secondFinger, zone));

        Assert.AreEqual(0, taps.Count, "A drag isn't a tap on any finger");
    }

    [Test]
    public void Should_Not_Tap_When_Finger_Joins_Press_In_Tap_Zone()
    {
        (TouchLookArea lookArea, List<int> taps) = makeLookAreaWithTapZone();
        Vector2 zone = screenPoint(lookArea, Vector2.zero);
        lookArea.OnPointerDown(makeEvent(_firstFinger, zone));

        lookArea.OnPointerDown(makeEvent(_secondFinger, zone));
        lookArea.OnPointerUp(makeEvent(_secondFinger, zone));
        lookArea.OnPointerUp(makeEvent(_firstFinger, zone));

        Assert.AreEqual(0, taps.Count, "Two fingers in the zone aren't a tap");
    }

    [Test]
    public void Should_Forget_Tap_When_Hidden()
    {
        (TouchLookArea lookArea, List<int> taps) = makeLookAreaWithTapZone();
        Vector2 zone = screenPoint(lookArea, Vector2.zero);
        lookArea.OnPointerDown(makeEvent(_firstFinger, screenPoint(lookArea, new Vector2(100, 0))));
        lookArea.OnPointerDown(makeEvent(_secondFinger, zone));

        lookArea.gameObject.SetActive(false);
        lookArea.gameObject.SetActive(true);
        lookArea.OnPointerUp(makeEvent(_secondFinger, zone));

        Assert.AreEqual(0, taps.Count, "A tap cut off by hiding shouldn't report anything");
    }

    #endregion

    #region WeaponCross

    [Test]
    public void Should_Select_Side_On_Tap()
    {
        (TouchWeaponCross cross, List<Vector2> slots) = makeCross();

        tap(cross, _firstFinger, new Vector2(100, 10));

        CollectionAssert.AreEqual(new[] { Vector2.right }, slots, "A tap should pick the side it's on");
    }

    [Test]
    public void Should_Select_Nothing_On_Center_Tap()
    {
        (TouchWeaponCross cross, List<Vector2> slots) = makeCross();

        tap(cross, _firstFinger, new Vector2(30, 30));

        CollectionAssert.IsEmpty(slots, "The center isn't on a side");
    }

    [Test]
    public void Should_Select_Once_On_Swipe()
    {
        (TouchWeaponCross cross, List<Vector2> slots) = makeCross();
        cross.OnPointerDown(makeEvent(_firstFinger, screenPoint(cross, Vector2.zero)));

        cross.OnDrag(makeEvent(_firstFinger, screenPoint(cross, new Vector2(0, 60))));
        cross.OnDrag(makeEvent(_firstFinger, screenPoint(cross, new Vector2(120, 60))));
        cross.OnPointerUp(makeEvent(_firstFinger, screenPoint(cross, new Vector2(120, 60))));

        CollectionAssert.AreEqual(new[] { Vector2.up }, slots, "Only the first long enough travel should pick");
    }

    [Test]
    public void Should_Select_By_Press_Position_After_Short_Drag()
    {
        (TouchWeaponCross cross, List<Vector2> slots) = makeCross();
        cross.OnPointerDown(makeEvent(_firstFinger, screenPoint(cross, new Vector2(-100, 0))));

        cross.OnDrag(makeEvent(_firstFinger, screenPoint(cross, new Vector2(-100, 20))));
        cross.OnPointerUp(makeEvent(_firstFinger, screenPoint(cross, new Vector2(-100, 20))));

        CollectionAssert.AreEqual(new[] { Vector2.left }, slots, "A short drag should still count as a tap");
    }

    [Test]
    public void Should_Follow_Only_First_Finger_On_Cross()
    {
        (TouchWeaponCross cross, List<Vector2> slots) = makeCross();
        cross.OnPointerDown(makeEvent(_firstFinger, screenPoint(cross, new Vector2(100, 0))));

        tap(cross, _secondFinger, new Vector2(-100, 0));
        cross.OnDrag(makeEvent(_secondFinger, screenPoint(cross, new Vector2(0, 100))));
        CollectionAssert.IsEmpty(slots, "A second finger shouldn't pick anything");

        cross.OnPointerUp(makeEvent(_firstFinger, screenPoint(cross, new Vector2(100, 0))));
        CollectionAssert.AreEqual(new[] { Vector2.right }, slots, "The first finger should still pick its side");
    }

    [Test]
    public void Should_Forget_Press_On_Cross_When_Hidden()
    {
        (TouchWeaponCross cross, List<Vector2> slots) = makeCross();
        cross.OnPointerDown(makeEvent(_firstFinger, screenPoint(cross, new Vector2(100, 0))));

        cross.gameObject.SetActive(false);
        cross.gameObject.SetActive(true);
        cross.OnPointerUp(makeEvent(_firstFinger, screenPoint(cross, new Vector2(100, 0))));
        CollectionAssert.IsEmpty(slots, "A press cut off by hiding shouldn't pick anything");

        tap(cross, _secondFinger, new Vector2(0, -100));
        CollectionAssert.AreEqual(new[] { Vector2.down }, slots, "The lost finger shouldn't keep others from the cross");
    }

    [Test]
    public void Should_Let_Only_Ring_Take_Pointers()
    {
        (TouchWeaponCross cross, _) = makeCross();

        Assert.IsTrue(cross.IsRaycastLocationValid(screenPoint(cross, new Vector2(100, 100)), null), "Inside the ring");
        Assert.IsFalse(cross.IsRaycastLocationValid(screenPoint(cross, new Vector2(140, 140)), null), "A corner of the rectangle");
    }

    #endregion

    #region Common

    [Test]
    public void Should_Respond_From_First_Pixel_Of_Drag()
    {
        IInitializePotentialDragHandler[] controls = { makeStick().Item1, makeControl<TouchLookArea>(), makeCross().Item1 };

        foreach (var control in controls)
        {
            PointerEventData eventData = makeEvent(_firstFinger, Vector2.zero);

            control.OnInitializePotentialDrag(eventData);

            Assert.IsFalse(eventData.useDragThreshold, $"{control.GetType().Name} should take drags without the usual dead zone");
        }
    }

    #endregion

    #region Helpers

    private T makeControl<T>() where T : MonoBehaviour
    {
        var control = new GameObject(typeof(T).Name, typeof(RectTransform));
        control.transform.SetParent(_root.transform, worldPositionStays: false);
        ((RectTransform)control.transform).sizeDelta = new Vector2(300, 300);
        return control.AddComponent<T>();
    }

    private (TouchStick, RectTransform, RectTransform) makeStick()
    {
        var stick = makeControl<TouchStick>();
        RectTransform view = makeChild("Stick", stick.transform);
        RectTransform knob = makeChild("Knob", view);
        setField(stick, "_stick", view);
        setField(stick, "_knob", knob);
        return (stick, view, knob);
    }

    private (TouchWeaponCross, List<Vector2>) makeCross()
    {
        var cross = makeControl<TouchWeaponCross>();
        List<Vector2> slots = new();
        cross.SlotSelected += slots.Add;
        return (cross, slots);
    }

    // Two fingers on the area, 400 pixels apart
    private (TouchLookArea, List<bool>) makePinchedLookArea()
    {
        TouchLookArea lookArea = makeControl<TouchLookArea>();
        List<bool> pinches = new();
        lookArea.Pinched += pinches.Add;
        lookArea.OnPointerDown(makeEvent(_firstFinger, Vector2.zero));
        lookArea.OnPointerDown(makeEvent(_secondFinger, new Vector2(400, 0)));
        return (lookArea, pinches);
    }

    private void moveFingers(TouchLookArea lookArea, float first, float second)
    {
        lookArea.OnDrag(makeEvent(_firstFinger, new Vector2(first, 0)));
        lookArea.OnDrag(makeEvent(_secondFinger, new Vector2(second, 0)));
    }

    // A zone 40 units wide on the area's center
    private (TouchLookArea, List<int>) makeLookAreaWithTapZone()
    {
        TouchLookArea lookArea = makeControl<TouchLookArea>();
        RectTransform zone = makeChild("TapZone", lookArea.transform);
        zone.sizeDelta = new Vector2(40, 40);
        setField(lookArea, "_tapZone", zone);
        List<int> taps = new();
        lookArea.ZoneTapped += () => taps.Add(1);
        return (lookArea, taps);
    }

    private RectTransform makeChild(string name, Transform parent)
    {
        var child = new GameObject(name, typeof(RectTransform));
        child.transform.SetParent(parent, worldPositionStays: false);
        return (RectTransform)child.transform;
    }

    private void setField(object target, string name, object value)
    {
        target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }

    private PointerEventData makeEvent(int pointerId, Vector2 position, Vector2 delta = default) => new(null)
    {
        pointerId = pointerId,
        position = position,
        delta = delta,
    };

    // An overlay canvas has its world space in screen pixels
    private Vector2 screenPoint(MonoBehaviour control, Vector2 local) => control.transform.TransformPoint(local);

    private void tap(TouchWeaponCross cross, int pointerId, Vector2 local)
    {
        cross.OnPointerDown(makeEvent(pointerId, screenPoint(cross, local)));
        cross.OnPointerUp(makeEvent(pointerId, screenPoint(cross, local)));
    }

    #endregion
}
