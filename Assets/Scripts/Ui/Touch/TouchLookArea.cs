using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

// The screen behind the other controls: dragging on it turns the camera, two fingers moving against each other
// pinch, and a tap on its tap zone is reported, leaving a drag started there to turn the camera like any other
public class TouchLookArea : TouchPointerArea
{
    // How much the gap between two fingers has to change to make a pinch (canvas units)
    [SerializeField] private float _pinchDistance = 80;

    // How far each of two fingers has to travel for them to count as pinching (canvas units)
    [SerializeField] private float _pinchStartTravel = 15;

    [SerializeField] private RectTransform _tapZone;

    // Travel that makes a press in the tap zone a drag (canvas units)
    [SerializeField] private float _tapDistance = 20;

    // The fingers down besides the followed one, by pointer
    private readonly Dictionary<int, Vector2> _otherFingers = new();

    private Canvas _canvas;

    private Vector2 _delta;

    private Vector2 _firstFinger;

    private Vector2 _firstFingerStart;

    private Vector2 _secondFingerStart;

    private int? _pinchPointerId;

    private bool _isPinching;

    // The extremes of the gap since the last report: a pinch counts from them, so turning back takes no extra travel
    private float _minGap;

    private float _maxGap;

    private int _followedPointerId;

    // The finger that may be tapping the tap zone, and where it pressed
    private int? _tapPointerId;

    private Vector2 _tapPress;

    // True for fingers coming together, false for moving apart
    public event Action<bool> Pinched;

    public event Action ZoneTapped;

    // The travel since the last call, in canvas units so a swipe turns as far on any screen density
    public Vector2 ConsumeDelta()
    {
        Vector2 delta = _delta;
        _delta = Vector2.zero;
        return delta;
    }

    protected override void onPress(PointerEventData eventData)
    {
        _firstFinger = eventData.position;
        _followedPointerId = eventData.pointerId;
        startTap(eventData);
    }

    protected override void onDrag(PointerEventData eventData)
    {
        _firstFinger = eventData.position;
        updateTap(eventData);
        updatePinch();
        if (!_isPinching) _delta += eventData.delta / _canvas.scaleFactor;
    }

    protected override void onRelease()
    {
        bool wasTap = _tapPointerId == _followedPointerId;
        if (wasTap) _tapPointerId = null;
        endPinch();
        if (_otherFingers.Count > 0) followNextFinger();
        else if (wasTap) ZoneTapped?.Invoke();
    }

    protected override void onOtherPress(PointerEventData eventData)
    {
        // A finger joining a press in the zone makes it a two-finger gesture, not a tap
        bool wasTapping = _tapPointerId != null;
        _tapPointerId = null;
        if (!wasTapping) startTap(eventData);
        _otherFingers[eventData.pointerId] = eventData.position;
        if (_pinchPointerId == null) startPinch(eventData.pointerId);
    }

    protected override void onOtherDrag(PointerEventData eventData)
    {
        if (!_otherFingers.ContainsKey(eventData.pointerId)) return;
        _otherFingers[eventData.pointerId] = eventData.position;
        updateTap(eventData);
        if (eventData.pointerId == _pinchPointerId) updatePinch();
    }

    protected override void onOtherRelease(PointerEventData eventData)
    {
        _otherFingers.Remove(eventData.pointerId);
        bool wasTap = eventData.pointerId == _tapPointerId;
        if (wasTap) _tapPointerId = null;
        if (eventData.pointerId == _pinchPointerId)
        {
            endPinch();
            if (_otherFingers.Count > 0) startPinch(_otherFingers.Keys.First());
        }
        if (wasTap) ZoneTapped?.Invoke();
    }

    protected override void onCancel()
    {
        _delta = Vector2.zero;
        _tapPointerId = null;
        _otherFingers.Clear();
        endPinch();
    }

    private void Awake() => _canvas = GetComponentInParent<Canvas>();

    private void startTap(PointerEventData eventData)
    {
        if (_tapZone == null || !RectTransformUtility.RectangleContainsScreenPoint(_tapZone, eventData.position, eventData.pressEventCamera)) return;
        _tapPointerId = eventData.pointerId;
        _tapPress = eventData.position;
    }

    private void updateTap(PointerEventData eventData)
    {
        if (eventData.pointerId != _tapPointerId) return;
        if (TouchUtils.IsSwipe(toCanvasUnits(eventData.position - _tapPress), _tapDistance)) _tapPointerId = null;
    }

    private void followNextFinger()
    {
        int pointerId = _otherFingers.Keys.First();
        _followedPointerId = pointerId;
        _firstFinger = _otherFingers[pointerId];
        _otherFingers.Remove(pointerId);
        adoptPointer(pointerId);
        if (_otherFingers.Count > 0) startPinch(_otherFingers.Keys.First());
    }

    private void startPinch(int pointerId)
    {
        _pinchPointerId = pointerId;
        _isPinching = false;
        _firstFingerStart = _firstFinger;
        _secondFingerStart = _otherFingers[pointerId];
        _minGap = _maxGap = getFingersGap();
    }

    private void endPinch()
    {
        _pinchPointerId = null;
        _isPinching = false;
    }

    private void updatePinch()
    {
        if (_pinchPointerId == null) return;
        float gap = getFingersGap();
        _minGap = Mathf.Min(_minGap, gap);
        _maxGap = Mathf.Max(_maxGap, gap);
        _isPinching = _isPinching || TouchUtils.IsPinchStart(
            toCanvasUnits(_firstFinger - _firstFingerStart),
            toCanvasUnits(getSecondFinger() - _secondFingerStart),
            _pinchStartTravel);
        if (!_isPinching) return;
        bool isTogether = TouchUtils.IsPinch(gap - _maxGap, _pinchDistance);
        if (!isTogether && !TouchUtils.IsPinch(gap - _minGap, _pinchDistance)) return;
        _minGap = _maxGap = gap;
        Pinched?.Invoke(isTogether);
    }

    private Vector2 getSecondFinger() => _otherFingers[_pinchPointerId.Value];

    private float getFingersGap() => toCanvasUnits(_firstFinger - getSecondFinger()).magnitude;

    private Vector2 toCanvasUnits(Vector2 pixels) => pixels / _canvas.scaleFactor;
}
