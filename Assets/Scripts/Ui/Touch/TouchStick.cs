using UnityEngine;
using UnityEngine.EventSystems;

// A zone of the screen that puts the stick under the finger pressing it, for as long as it's held
public class TouchStick : TouchPointerArea
{
    [SerializeField] private RectTransform _stick;

    [SerializeField] private RectTransform _knob;

    // How far the knob travels from the centre (canvas units)
    [SerializeField] private float _radius = 75;

    public Vector2 Value { get; private set; }

    protected override void onPress(PointerEventData eventData)
    {
        _stick.anchoredPosition = toLocal(eventData);
        _stick.gameObject.SetActive(true);
    }

    protected override void onDrag(PointerEventData eventData)
    {
        Value = TouchUtils.ToStickValue(toLocal(eventData) - _stick.anchoredPosition, _radius);
        _knob.anchoredPosition = Value * _radius;
    }

    protected override void onRelease() => resetStick();

    protected override void onCancel() => resetStick();

    private void resetStick()
    {
        Value = Vector2.zero;
        _knob.anchoredPosition = Vector2.zero;
        _stick.gameObject.SetActive(false);
    }
}
