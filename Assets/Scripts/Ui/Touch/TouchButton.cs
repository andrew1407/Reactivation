using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private const float _pressedScale = 0.92f;

    private int? _pointerId;

    public event Action Pressed;

    public event Action Released;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_pointerId != null) return;
        _pointerId = eventData.pointerId;
        transform.localScale = Vector3.one * _pressedScale;
        Pressed?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId == _pointerId) release();
    }

    // Hidden under a finger: the up event won't come
    private void OnDisable() => release();

    private void release()
    {
        if (_pointerId == null) return;
        _pointerId = null;
        transform.localScale = Vector3.one;
        Released?.Invoke();
    }
}
