using UnityEngine;
using UnityEngine.EventSystems;

// The screen behind the other controls: dragging on it turns the camera
public class TouchLookArea : TouchPointerArea
{
    private Canvas _canvas;

    private Vector2 _delta;

    // The travel since the last call, in canvas units so a swipe turns as far on any screen density
    public Vector2 ConsumeDelta()
    {
        Vector2 delta = _delta;
        _delta = Vector2.zero;
        return delta;
    }

    protected override void onDrag(PointerEventData eventData) => _delta += eventData.delta / _canvas.scaleFactor;

    protected override void onCancel() => _delta = Vector2.zero;

    private void Awake() => _canvas = GetComponentInParent<Canvas>();
}
