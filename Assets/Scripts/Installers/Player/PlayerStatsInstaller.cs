using UnityEngine;
using Zenject;

public class PlayerStatsInstaller : MonoInstaller
{
    [Header("Health")]

    [SerializeField] private int _healthPoints;

    [SerializeField] private float _regenerationDelay;

    [SerializeField] private int _regenerationPoints;

    public override void InstallBindings()
    {
        Container.Bind<HealthContainer>()
            .AsSingle()
            .WithArguments(_healthPoints);

        Container.BindInterfacesAndSelfTo<PlayerHealthController>()
            .FromSubContainerResolve()
            .ByMethod(installHealthController)
            .AsSingle();
    }

    private void installHealthController(DiContainer subContainer)
    {
        subContainer.Bind<PlayerHealthController>().AsSingle();
        subContainer.BindInstance(_regenerationDelay).WithId("RegenerationDelay");
        subContainer.BindInstance(_regenerationPoints).WithId("RegenerationPoints");
    }
}