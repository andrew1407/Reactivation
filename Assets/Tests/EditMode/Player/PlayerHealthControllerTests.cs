using NUnit.Framework;
using Zenject;

public class PlayerHealthControllerTests : ZenjectUnitTestFixture
{
    private const int MaxHealth = 100;

    private UiControllerStub _ui;

    private HealthContainer _health;

    [SetUp]
    public void SetUpUi()
    {
        _ui = new UiControllerStub();
        _health = new HealthContainer(MaxHealth);
        Container.BindInstance(_ui.Controller);
        Container.BindInstance(_health);
        Container.Bind<PlayerStatsObserver>().AsSingle();
    }

    [TearDown]
    public void TearDownUi() => _ui.Destroy();

    [Test]
    public void Should_Take_Damage_And_Show_Health()
    {
        PlayerHealthController controller = createController(regenerationDelay: 1, regenerationPoints: 5);

        controller.TakeDamage(DamageType.FIST, 30);

        Assert.AreEqual(70, _health.HealthPoints);
        Assert.AreEqual("70", _ui.Text("_health"));
    }

    [Test]
    public void Should_Not_Regenerate_While_Inactive()
    {
        PlayerHealthController controller = createController(regenerationDelay: 0, regenerationPoints: 5);
        controller.TakeDamage(DamageType.FIST, 30);

        controller.Tick();

        Assert.AreEqual(70, _health.HealthPoints);
    }

    [Test]
    public void Should_Regenerate_After_Delay()
    {
        PlayerHealthController controller = createController(regenerationDelay: 0, regenerationPoints: 5);
        controller.TakeDamage(DamageType.FIST, 30);

        controller.RegenerationActive = true;
        controller.Tick();

        Assert.AreEqual(75, _health.HealthPoints);
        Assert.AreEqual("75", _ui.Text("_health"));
    }

    [Test]
    public void Should_Not_Regenerate_Before_Delay()
    {
        PlayerHealthController controller = createController(regenerationDelay: float.MaxValue, regenerationPoints: 5);
        controller.TakeDamage(DamageType.FIST, 30);

        controller.RegenerationActive = true;
        controller.Tick();

        Assert.AreEqual(70, _health.HealthPoints);
    }

    [Test]
    public void Should_Not_Regenerate_Above_Max_Health()
    {
        PlayerHealthController controller = createController(regenerationDelay: 0, regenerationPoints: 50);
        controller.TakeDamage(DamageType.FIST, 10);

        controller.RegenerationActive = true;
        controller.Tick();

        Assert.AreEqual(MaxHealth, _health.HealthPoints);
    }

    private PlayerHealthController createController(float regenerationDelay, int regenerationPoints)
    {
        Container.BindInstance(regenerationDelay).WithId("RegenerationDelay");
        Container.BindInstance(regenerationPoints).WithId("RegenerationPoints");
        Container.Bind<PlayerHealthController>().AsSingle();
        return Container.Resolve<PlayerHealthController>();
    }
}
