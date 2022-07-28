using UnityEngine;
using Zenject;

public class WavesControllerInstaller : MonoInstaller
{
    [SerializeField] private uint _waveDurationSeconds;
    
    [SerializeField] private uint _wavesCount;

    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<WavesController>()
            .FromSubContainerResolve()
            .ByMethod(installWaveController)
            .AsSingle();

        Container.Bind<Timer>()
            .AsSingle()
            .WithArguments(_waveDurationSeconds);

        Container.Bind<WavesStartObserver>().AsSingle();
    }

    private void installWaveController(DiContainer subContainer)
    {
        subContainer.Bind<WavesController>().AsSingle();
        subContainer.BindInstance(_wavesCount);
    }
}