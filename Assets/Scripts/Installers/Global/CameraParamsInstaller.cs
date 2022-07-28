using UnityEngine;
using Zenject;

public class CameraParamsInstaller : MonoInstaller
{
    [SerializeField] private CameraParams _cameraParams;

    public override void InstallBindings()
    {
        Container.BindInstance(_cameraParams);
    }
}