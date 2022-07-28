using System.Collections.Generic;
using UnityEngine;

public class InfoMessageViewer : MonoBehaviour
{
    [SerializeField] private GameObject _playMessage;

    [SerializeField] private GameObject _winMessage;

    [SerializeField] private GameObject _defeatMessage;

    [SerializeField] private GameObject _noTimeMessage;

    [SerializeField] private GameObject _pauseMessage;

    [SerializeField] private GameObject _controlsPannel;

    private Dictionary<WaveState, GameObject> _infoTabs;

    public bool PauseTabActive
    {
        get => _pauseMessage.activeSelf;
        set => _pauseMessage.SetActive(value);
    }

    public bool ControlsTabActive
    {
        get => _controlsPannel.activeSelf;
        set => _controlsPannel.SetActive(value);
    }

    public void ShowTab(WaveState type)
    {
        foreach (var tab in _infoTabs) tab.Value.SetActive(type == tab.Key);
    }

    public void HideAllTAbs()
    {
        foreach (var tab in _infoTabs.Values) tab.SetActive(false);
    }

    private void Awake()
    {
        _infoTabs = new() {
            {WaveState.IDLE, _playMessage},
            {WaveState.TIME_IS_OVER, _noTimeMessage},
            {WaveState.DIED, _defeatMessage},
            {WaveState.ALL_WAVES_SURVIVED, _winMessage},
        };
    }
}
