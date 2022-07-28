using UnityEngine;
using Unity.Barracuda;
using Zenject;

public class DamageModifiersInstaller : MonoInstaller
{
    [SerializeField] private NNModel _kerasModel;

    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<DamageModifiersSystem>()
            .FromSubContainerResolve()
            .ByMethod(installModifiersSystem)
            .AsSingle();
    }

    private void installModifiersSystem(DiContainer subContainer)
    {
        subContainer.Bind<DamageModifiersSystem>().AsSingle();

        subContainer.BindInterfacesAndSelfTo<RandomDamageModifier>().AsSingle();
        subContainer.BindInterfacesAndSelfTo<OneWaveDamageModifier>().AsSingle();
        subContainer.BindInterfacesAndSelfTo<DamageModifierPredictor>()
            .FromInstance(DamageModifierPredictor.FromModel(_kerasModel))
            .AsSingle();
    }
}