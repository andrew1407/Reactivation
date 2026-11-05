using UnityEngine;
using Zenject;

// Text sized for a monitor is too small to read on a phone: grows this element on mobile and while on the
// touch controls, moving or resizing it where that would run it into its neighbours
public class TouchEnlarged : MonoBehaviour
{
    [SerializeField] private float _scale = 2;

    [SerializeField] private Vector2 _touchPosition;

    [SerializeField] private Vector2 _touchSize;

    [Inject] private readonly TouchMode _touchMode;

    private RectTransform _rect;

    private Vector3 _defaultScale;

    private Vector2 _defaultPosition;

    private Vector2 _defaultSize;

    private void Awake()
    {
        _rect = (RectTransform)transform;
        _defaultScale = _rect.localScale;
        _defaultPosition = _rect.anchoredPosition;
        _defaultSize = _rect.sizeDelta;
    }

    private void OnEnable()
    {
        _touchMode.Changed += onTouchChanged;
        apply();
    }

    private void OnDisable() => _touchMode.Changed -= onTouchChanged;

    private void onTouchChanged(bool _) => apply();

    private void apply()
    {
        bool isEnlarged = _touchMode.IsTextEnlarged;
        _rect.localScale = isEnlarged ? _defaultScale * _scale : _defaultScale;
        _rect.anchoredPosition = isEnlarged ? _touchPosition : _defaultPosition;
        _rect.sizeDelta = isEnlarged ? _touchSize : _defaultSize;
    }
}
