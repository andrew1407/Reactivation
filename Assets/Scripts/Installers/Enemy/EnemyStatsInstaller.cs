using UnityEngine;
using Zenject;
using TMPro;

public class EnemyStatsInstaller : MonoInstaller
{
    [Header("Health")]

    [SerializeField] private int _healthPoints;

    [SerializeField] private GameObject _healthTab;

    public override void InstallBindings()
    {
        Container.Bind<HealthContainer>()
            .FromSubContainerResolve()
            .ByMethod(installHealthContainer)
            .AsSingle();

        Container.Bind<EnemyHealthController>()
            .FromSubContainerResolve()
            .ByMethod(installHealthController)
            .AsSingle();
    }

    private void installHealthContainer(DiContainer subContainer)
    {
        subContainer.Bind<HealthContainer>().AsSingle();
        subContainer.BindInstance(_healthPoints);
    }

    private void installHealthController(DiContainer subContainer)
    {
        subContainer.Bind<EnemyHealthController>().AsSingle();
        var text = _healthTab.GetComponent<TextMeshProUGUI>();
        var transform = _healthTab.GetComponent<RectTransform>();
        subContainer.BindInstances(text, transform);
    }
}