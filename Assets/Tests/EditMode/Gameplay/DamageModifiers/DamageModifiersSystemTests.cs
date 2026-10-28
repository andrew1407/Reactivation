using System;
using System.Collections.Generic;
using NUnit.Framework;
using Zenject;

using DamageModifications = System.Collections.Generic.Dictionary<DamageType, float>;

public class DamageModifiersSystemTests : ZenjectUnitTestFixture
{
    private UiControllerStub _ui;

    [SetUp]
    public void SetUpUi()
    {
        _ui = new UiControllerStub();
        Container.BindInstance(_ui.Controller);
    }

    [TearDown]
    public void TearDownUi() => _ui.Destroy();

    [Test]
    public void Should_Modify_Damage_With_Picked_Modifier()
    {
        DamageModifiersSystem system = createSystem(new ModifierMock(new() { { DamageType.ARROW, 0.5f } }));

        system.PickModifier();

        Assert.AreEqual(5, system.ModifyDamage(DamageType.ARROW, 10));
    }

    [Test]
    public void Should_Keep_Damage_Of_Types_Without_Modification()
    {
        DamageModifiersSystem system = createSystem(new ModifierMock(new() { { DamageType.ARROW, 0.5f } }));

        system.PickModifier();

        Assert.AreEqual(10, system.ModifyDamage(DamageType.FIST, 10));
    }

    [Test]
    public void Should_Pick_Single_Modifier_Repeatedly()
    {
        ModifierMock modifier = new(new() { { DamageType.GRENADE, 0.25f } });
        DamageModifiersSystem system = createSystem(modifier);

        system.PickModifier();
        system.PickModifier();

        Assert.AreEqual(1, system.ModifyDamage(DamageType.GRENADE, 4));
    }

    [Test]
    public void Should_Not_Pick_Previous_Modifier_Again()
    {
        ModifierMock half = new(new() { { DamageType.MACHETE, 0.5f } });
        ModifierMock quarter = new(new() { { DamageType.MACHETE, 0.25f } });
        DamageModifiersSystem system = createSystem(half, quarter);

        system.PickModifier();
        float previous = system.ModifyDamage(DamageType.MACHETE, 100);
        for (int i = 0; i < 10; ++i)
        {
            system.PickModifier();
            float current = system.ModifyDamage(DamageType.MACHETE, 100);
            Assert.AreNotEqual(previous, current, $"Pick {i + 1} repeated the previous modifier");
            previous = current;
        }
    }

    [Test]
    public void Should_Pass_Collector_To_Picked_Modifier()
    {
        ModifierMock modifier = new(GameUtils.DefaultModifications());
        DamageModifiersSystem system = createSystem(modifier);
        PlayerKillsCollector collector = new();

        system.PickModifier();
        system.UseCollectorData(collector);

        Assert.AreSame(collector, modifier.AnalyzedCollector);
    }

    [Test]
    public void Should_Reset_Picked_Modifier_Data()
    {
        ModifierMock modifier = new(GameUtils.DefaultModifications());
        DamageModifiersSystem system = createSystem(modifier);

        system.ClearModifierData();
        Assert.AreEqual(0, modifier.ResetCalled, "Nothing to reset before a modifier is picked");

        system.PickModifier();
        system.ClearModifierData();
        Assert.AreEqual(1, modifier.ResetCalled);
    }

    [Test]
    public void Should_Dispose_Disposable_Modifiers()
    {
        DisposableModifierMock disposable = new(GameUtils.DefaultModifications());
        DamageModifiersSystem system = createSystem(new ModifierMock(GameUtils.DefaultModifications()), disposable);

        system.LateDispose();

        Assert.AreEqual(1, disposable.DisposeCalled);
    }

    private DamageModifiersSystem createSystem(params IDamageModifier[] modifiers)
    {
        foreach (var modifier in modifiers) Container.Bind<IDamageModifier>().FromInstance(modifier);
        Container.Bind<DamageModifiersSystem>().AsSingle();
        return Container.Resolve<DamageModifiersSystem>();
    }

    private class ModifierMock : IDamageModifier
    {
        public ICollector AnalyzedCollector;

        public int ResetCalled = 0;

        private readonly DamageModifications _modifications;

        public ModifierMock(DamageModifications modifications) => _modifications = modifications;

        public DamageModifications GetModifications() => _modifications;

        public void AnalyzeWaveStats(ICollector collector) => AnalyzedCollector = collector;

        public void ResetModifications() => ++ResetCalled;
    }

    private class DisposableModifierMock : ModifierMock, IDisposable
    {
        public int DisposeCalled = 0;

        public DisposableModifierMock(DamageModifications modifications) : base(modifications) {}

        public void Dispose() => ++DisposeCalled;
    }
}
