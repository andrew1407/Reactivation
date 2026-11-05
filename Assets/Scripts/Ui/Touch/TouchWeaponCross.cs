using System;
using UnityEngine;
using UnityEngine.EventSystems;

// Picks a weapon by tapping a side of the ring or swiping towards it from anywhere on it.
// Reports the same directions as the gamepad's cross
public class TouchWeaponCross : TouchPointerArea, ICanvasRaycastFilter
{
    // Travel that makes a press a swipe (canvas units)
    [SerializeField] private float _swipeDistance = 50;

    // A tap this close to the centre is on the arrows, not on a side (canvas units)
    [SerializeField] private float _centerRadius = 75;

    private Vector2 _pressPosition;

    private bool _wasSwiped;

    public event Action<Vector2> SlotSelected;

    // The corners of its rectangle stay free for turning the camera
    public bool IsRaycastLocationValid(Vector2 screenPosition, Camera eventCamera)
    {
        var rect = (RectTransform)transform;
        return TouchUtils.IsOnRing(toLocal(screenPosition, eventCamera), rect.rect.width / 2);
    }

    protected override void onPress(PointerEventData eventData)
    {
        _pressPosition = toLocal(eventData);
        _wasSwiped = false;
    }

    protected override void onDrag(PointerEventData eventData)
    {
        if (_wasSwiped) return;
        Vector2 travel = toLocal(eventData) - _pressPosition;
        if (!TouchUtils.IsSwipe(travel, _swipeDistance)) return;
        _wasSwiped = true;
        SlotSelected?.Invoke(TouchUtils.ToWeaponSlot(travel));
    }

    protected override void onRelease()
    {
        if (!_wasSwiped && _pressPosition.magnitude > _centerRadius) SlotSelected?.Invoke(TouchUtils.ToWeaponSlot(_pressPosition));
    }
}
