using NUnit.Framework;
using Zenject;

public class PlayerHealthControllerTests : ZenjectUnitTestFixture
{
    private const int _maxHealth = 100;

    private UiControllerStub _ui;

    private HealthContainer _health;

    [SetUp]
    public void SetUpUi()
    {
        _ui = new UiControllerStub();
        _health = new HealthContainer(_maxHealth);
        Container.BindInstance(_ui.Controller);
        Container.BindInstance(_health);
        Container.Bind<PlayerStatsObserver>().AsSingle();
    }

    [TearDown]
    public void TearDownUi() => _ui.Destroy();

    [Test]
    public void Should_Take_Damage_And_Show_Health()
    {
        PlayerHealthController controller = makeController(regenerationDelay: 1, regenerationPoints: 5);

        controller.TakeDamage(DamageType.FIST, 30);

        Assert.AreEqual(70, _health.HealthPoints, "Damage should take points");
        Assert.AreEqual("70", _ui.Text("_health"), "The HUD should show the health left");
    }

    [Test]
    public void Should_Not_Regenerate_While_Inactive()
    {
        PlayerHealthController controller = makeController(regenerationDelay: 0, regenerationPoints: 5);
        controller.TakeDamage(DamageType.FIST, 30);

        controller.Tick();

        Assert.AreEqual(70, _health.HealthPoints, "Inactive regeneration shouldn't heal");
    }

    [Test]
    public void Should_Regenerate_After_Delay()
    {
        PlayerHealthController controller = makeController(regenerationDelay: 0, regenerationPoints: 5);
        controller.TakeDamage(DamageType.FIST, 30);

        controller.RegenerationActive = true;
        controller.Tick();

        Assert.AreEqual(75, _health.HealthPoints, "Regeneration should add its points");
        Assert.AreEqual("75", _ui.Text("_health"), "The HUD should show the regenerated health");
    }

    [Test]
    public void Should_Not_Regenerate_Before_Delay()
    {
        PlayerHealthController controller = makeController(regenerationDelay: float.MaxValue, regenerationPoints: 5);
        controller.TakeDamage(DamageType.FIST, 30);

        controller.RegenerationActive = true;
        controller.Tick();

        Assert.AreEqual(70, _health.HealthPoints, "Nothing should heal before the delay");
    }

    [Test]
    public void Should_Not_Regenerate_Above_Max_Health()
    {
        PlayerHealthController controller = makeController(regenerationDelay: 0, regenerationPoints: 50);
        controller.TakeDamage(DamageType.FIST, 10);

        controller.RegenerationActive = true;
        controller.Tick();

        Assert.AreEqual(_maxHealth, _health.HealthPoints, "Regeneration should stop at the maximum");
    }

    private PlayerHealthController makeController(float regenerationDelay, int regenerationPoints)
    {
        Container.BindInstance(regenerationDelay).WithId("RegenerationDelay");
        Container.BindInstance(regenerationPoints).WithId("RegenerationPoints");
        Container.Bind<PlayerHealthController>().AsSingle();
        return Container.Resolve<PlayerHealthController>();
    }
}
