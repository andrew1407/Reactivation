using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

// Makes the pause message a button on touch: tapping it resumes the game
public class TouchPauseTab : MonoBehaviour, IPointerClickHandler
{
    [Inject] private readonly TouchMode _touchMode;

    [Inject] private readonly GameManager _gameManager;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_touchMode.IsActive && _gameManager.GamePaused) _gameManager.SwitchPauseState();
    }
}
