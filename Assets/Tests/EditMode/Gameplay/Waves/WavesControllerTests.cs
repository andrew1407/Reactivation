using NUnit.Framework;
using Zenject;

public class WavesControllerTests : ZenjectUnitTestFixture
{
    private const uint WavesCount = 3;

    private const uint WaveSeconds = 60;

    private UiControllerStub _ui;

    private Timer _timer;

    private WavesController _controller;

    [SetUp]
    public void SetUpController()
    {
        _ui = new UiControllerStub();
        _timer = new Timer(WaveSeconds);
        Container.BindInstance(_ui.Controller);
        Container.BindInstance(_timer);
        Container.BindInstance(WavesCount);
        // Bound as is, without injection: the tests never let the timer run out
        Container.BindInstance(new PlayerActivityObserver());
        Container.Bind<WavesController>().AsSingle();
        _controller = Container.Resolve<WavesController>();
    }

    [TearDown]
    public void TearDownUi() => _ui.Destroy();

    [Test]
    public void Should_Start_From_First_Wave()
    {
        _controller.ResetParameters();

        Assert.AreEqual("1", _ui.Text("_wave"));
        Assert.AreEqual(WaveSeconds.ToString(), _ui.Text("_timer"));
        Assert.IsFalse(_controller.FinalWave);
    }

    [Test]
    public void Should_Show_Next_Wave()
    {
        _controller.SetNextWave();

        Assert.AreEqual("2", _ui.Text("_wave"));
    }

    [Test]
    public void Should_Be_Final_Wave_After_All_Waves_Started()
    {
        for (int i = 1; i < WavesCount; ++i)
        {
            Assert.IsFalse(_controller.FinalWave, $"Wave {i} of {WavesCount} isn't final");
            _controller.SetNextWave();
        }

        Assert.IsTrue(_controller.FinalWave);
    }

    [Test]
    public void Should_Restart_From_First_Wave_On_Reset()
    {
        _controller.SetNextWave();
        _controller.SetNextWave();

        _controller.ResetParameters();

        Assert.AreEqual("1", _ui.Text("_wave"));
        Assert.IsFalse(_controller.FinalWave);
    }

    [Test]
    public void Should_Not_Tick_Inactive_Timer()
    {
        string shown = _ui.Text("_timer");

        _controller.Tick();

        Assert.AreEqual(WaveSeconds, _timer.CurrentTime);
        Assert.AreEqual(shown, _ui.Text("_timer"), "The HUD shouldn't update while the timer is stopped");
    }

    [Test]
    public void Should_Show_Time_Left_On_Tick()
    {
        _timer.Active = true;

        _controller.Tick();

        Assert.AreEqual(_timer.CurrentTime.ToString("0"), _ui.Text("_timer"));
    }
}
