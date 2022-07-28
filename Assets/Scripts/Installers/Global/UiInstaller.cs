using UnityEngine;
using Zenject;

public class UiInstaller : MonoInstaller
{
    [SerializeField] private InfoMessageViewer _infoMessageViewer;

    [SerializeField] private UiController _uiController;

    public override void InstallBindings()
    {
        Container.BindInstances(_infoMessageViewer, _uiController);
    }
}