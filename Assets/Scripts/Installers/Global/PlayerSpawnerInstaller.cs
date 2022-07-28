using UnityEngine;
using Zenject;

public class PlayerSpawnerInstaller : MonoInstaller
{
    [SerializeField] private GameObject _playerPrefab;

    [SerializeField] private Transform _spawnPosition;

    [SerializeField] private Transform _ammoPreloadPosition;

    public override void InstallBindings()
    {
        Container.Bind<PlayerSpawner>()
            .FromSubContainerResolve()
            .ByMethod(insatllPlayerSpawner)
            .AsSingle();

        installPlayerUsageTools();
    }

    private void installPlayerUsageTools()
    {
        Container.BindInstance(_ammoPreloadPosition)
            .WithId("PreloadPosition")
            .WhenInjectedInto<IAmmoManager>();

        Container.Bind<PlayerAmmoObserver>().AsSingle();
        Container.Bind<WeaponObserver>().AsSingle();
        Container.Bind<PlayerStatsObserver>().AsSingle();
        Container.Bind<PlayerActivityObserver>().AsSingle();
    }

    private void insatllPlayerSpawner(DiContainer subContainer)
    {
        subContainer.Bind<PlayerSpawner>().AsSingle();
        subContainer.BindFactory<Player, Player.Factory>()
            .FromSubContainerResolve()
            .ByNewContextPrefab(_playerPrefab);

        subContainer.BindInstance(_spawnPosition);
    }
}