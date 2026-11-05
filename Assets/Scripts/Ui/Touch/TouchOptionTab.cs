using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

// Makes a start/replay message a button on touch, where there's no key for its prompt to name
public class TouchOptionTab : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TextMeshProUGUI _message;

    [SerializeField, TextArea] private string _touchMessage;

    [SerializeField] private float _touchFontSize;

    [Inject] private readonly TouchMode _touchMode;

    [Inject] private readonly PlayOptionsSwitch _playOptionsSwitch;

    private string _defaultMessage;

    private float _defaultFontSize;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_touchMode.IsActive) _playOptionsSwitch.ActivateOption();
    }

    private void Awake()
    {
        _defaultMessage = _message.text;
        _defaultFontSize = _message.fontSize;
    }

    private void OnEnable()
    {
        _touchMode.Changed += showMessage;
        showMessage(_touchMode.IsActive);
    }

    private void OnDisable() => _touchMode.Changed -= showMessage;

    private void showMessage(bool isTouch)
    {
        _message.text = isTouch ? _touchMessage : _defaultMessage;
        _message.fontSize = isTouch ? _touchFontSize : _defaultFontSize;
    }
}
