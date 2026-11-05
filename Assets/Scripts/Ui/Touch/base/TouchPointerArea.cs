using UnityEngine;
using UnityEngine.EventSystems;

// A part of the screen that follows the finger pressing it first, ignoring any other until that one lifts
public abstract class TouchPointerArea : MonoBehaviour, IInitializePotentialDragHandler, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private int? _pointerId;

    public void OnInitializePotentialDrag(PointerEventData eventData) => eventData.useDragThreshold = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_pointerId != null) return;
        _pointerId = eventData.pointerId;
        onPress(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerId == _pointerId) onDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId != _pointerId) return;
        _pointerId = null;
        onRelease();
    }

    protected virtual void onPress(PointerEventData eventData) {}

    protected abstract void onDrag(PointerEventData eventData);

    protected virtual void onRelease() {}

    // Hidden, maybe under a finger: the up event won't come
    protected virtual void onCancel() {}

    protected Vector2 toLocal(PointerEventData eventData) => toLocal(eventData.position, eventData.pressEventCamera);

    protected Vector2 toLocal(Vector2 screenPosition, Camera eventCamera)
    {
        var rect = (RectTransform)transform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPosition, eventCamera, out Vector2 position);
        return position;
    }

    private void OnDisable()
    {
        _pointerId = null;
        onCancel();
    }
}
