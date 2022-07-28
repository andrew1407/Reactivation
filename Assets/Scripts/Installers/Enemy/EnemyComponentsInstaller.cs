using UnityEngine;
using UnityEngine.AI;
using Zenject;

public class EnemyComponentsInstaller : MonoInstaller
{
    [SerializeField] private CharacterComponents _characterComponents;

    [SerializeField] private NavMeshAgent _navMeshAgent;

    [SerializeField] private float _attackDistance;

    [SerializeField] private float _attackFrequency;

    public override void InstallBindings()
    {
        Container.BindInstance(_characterComponents);
        Container.Bind<EnemiesActivityObserver>().AsSingle();

        Container.Bind<EnemyActionsStrategy>()
            .FromSubContainerResolve()
            .ByMethod(installEnemyActionsStrategy)
            .AsSingle();
    }

    private void installEnemyActionsStrategy(DiContainer subContainer)
    {
        subContainer.Bind<EnemyActionsStrategy>().AsSingle();
        subContainer.BindInstance(_navMeshAgent);
        subContainer.BindInstance(_attackDistance).WithId("AttackDistance");
        subContainer.BindInstance(_attackFrequency).WithId("AttackFrequency");
    }
}