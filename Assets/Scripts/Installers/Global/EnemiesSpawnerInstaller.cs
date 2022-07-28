using UnityEngine;
using Zenject;

public class EnemiesSpawnerInstaller : MonoInstaller
{
    [SerializeField] private GameObject _enemyPrefab;

    [SerializeField] private Transform _spawnPosition;

    [SerializeField] private uint _enemiesCount;

    public override void InstallBindings()
    {
        Container.Bind<EnemiesSpawner>()
            .FromSubContainerResolve()
            .ByMethod(installEnemiesSpawner)
            .AsSingle();
    }

    private void installEnemiesSpawner(DiContainer subContainer)
    {
        subContainer.Bind<EnemiesSpawner>().AsSingle();
        subContainer.BindFactory<Enemy, Enemy.Factory>()
            .FromSubContainerResolve()
            .ByNewContextPrefab(_enemyPrefab);
        
        subContainer.BindInstances(_spawnPosition, _enemiesCount);
    }
}