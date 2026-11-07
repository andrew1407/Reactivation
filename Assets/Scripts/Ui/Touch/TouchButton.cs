using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private const float _pressedScale = 0.92f;

    private int? _pointerId;

    private float _size = 1;

    public event Action Pressed;

    public event Action Released;

    public void Resize(float size)
    {
        _size = size;
        applyScale();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_pointerId != null) return;
        _pointerId = eventData.pointerId;
        applyScale();
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
        applyScale();
        Released?.Invoke();
    }

    private void applyScale() => transform.localScale = Vector3.one * (_pointerId == null ? _size : _size * _pressedScale);
}
