using UnityEngine;
using Zenject;

public class GameManagerInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<PlayOptionsSwitch>().AsSingle();
        Container.Bind<PlayerKillsCollector>().AsSingle();
        Container.Bind<GameManager>().AsSingle();
    }
}