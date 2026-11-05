using UnityEngine;
using Zenject;

public class UiInstaller : MonoInstaller
{
    [SerializeField] private InfoMessageViewer _infoMessageViewer;

    [SerializeField] private UiController _uiController;

    [SerializeField] private TouchControls _touchControls;

    public override void InstallBindings()
    {
        Container.BindInstances(_infoMessageViewer, _uiController, _touchControls);
        Container.BindInstance(TouchMode.ForCurrentPlatform());
    }
}