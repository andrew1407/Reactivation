using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

// The on-screen controls as wired into the real level. A desktop run keeps them hidden and touch mode off,
// so the buttons get the calls the event system would make and the hidden areas' events are raised directly
public class TouchInGameTests
{
    private const string _mainScene = "MainScene";

    private const float _spawnTimeoutSeconds = 10;

    private const int _finger = 1;

    private const BindingFlags _private = BindingFlags.Instance | BindingFlags.NonPublic;

    private Player _player;

    private TouchControls _touchControls;

    private GameManager _gameManager;

    private TouchMode _touchMode;

    private CameraMovement _cameraMovement;

    private GameObject _aimHighlight;

    [UnitySetUp]
    public IEnumerator LoadLevel()
    {
        yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(_mainScene);

        float waited = 0;
        while (_player == null && waited < _spawnTimeoutSeconds)
        {
            yield return null;
            waited += Time.unscaledDeltaTime;
            _player = Object.FindAnyObjectByType<Player>();
        }
        Assert.IsNotNull(_player, $"The player wasn't spawned within {_spawnTimeoutSeconds}s of loading {_mainScene}");
        yield return null;

        var sceneManager = Object.FindAnyObjectByType<SceneManager>();
        var inputController = Object.FindAnyObjectByType<PlayerInputController>();
        _touchControls = Object.FindAnyObjectByType<TouchControls>();
        _gameManager = getField<GameManager>(sceneManager, "_gameManager");
        _touchMode = getField<TouchMode>(sceneManager, "_touchMode");
        _cameraMovement = getField<CameraMovement>(inputController, "_cameraMovement");
        _aimHighlight = getField<GameObject>(_touchControls, "_aimHighlight");
    }

    [TearDown]
    public void ResumeTime()
    {
        Time.timeScale = 1;
        _player = null;
    }

    #region Aiming

    [Test]
    public void Should_Switch_Aiming_With_Aim_Button()
    {
        pressAim();
        Assert.IsTrue(_cameraMovement.Aiming, "The first press should aim");
        Assert.IsTrue(_aimHighlight.activeSelf, "The button should show it's aiming");

        pressAim();
        Assert.IsFalse(_cameraMovement.Aiming, "The second press should stop aiming");
        Assert.IsFalse(_aimHighlight.activeSelf, "The button should show it isn't aiming");
    }

    [Test]
    public void Should_Aim_By_Pinching_Together_And_Stop_By_Moving_Apart()
    {
        raise(_touchControls.LookArea, "Pinched", true);
        Assert.IsTrue(_cameraMovement.Aiming, "Fingers coming together should aim");

        raise(_touchControls.LookArea, "Pinched", false);
        Assert.IsFalse(_cameraMovement.Aiming, "Fingers moving apart should stop aiming");
    }

    [Test]
    public void Should_Not_Change_Aiming_While_Paused()
    {
        _gameManager.SwitchPauseState();

        pressAim();
        raise(_touchControls.LookArea, "Pinched", true);

        Assert.IsFalse(_cameraMovement.Aiming, "A paused game shouldn't start aiming");
        Assert.IsFalse(_aimHighlight.activeSelf, "The button shouldn't show aiming either");
    }

    [UnityTest]
    public IEnumerator Should_Start_Without_Aiming_After_Being_Deactivated_While_Aiming()
    {
        pressAim();

        _player.gameObject.SetActive(false);
        Assert.IsFalse(_aimHighlight.activeSelf, "The button should stop showing aiming on death");

        _player.gameObject.SetActive(true);
        yield return null;
        Assert.IsFalse(_cameraMovement.Aiming, "A respawned player shouldn't be left aiming");

        pressAim();
        Assert.IsTrue(_cameraMovement.Aiming, "The first press after respawning should aim");
    }

    [Test]
    public void Should_Stop_Touch_Aiming_Only_When_Leaving_Touch()
    {
        pressAim();

        raise(_touchMode, "Changed", true);
        Assert.IsTrue(_cameraMovement.Aiming, "Coming back to touch shouldn't stop aiming");

        raise(_touchMode, "Changed", false);
        Assert.IsFalse(_cameraMovement.Aiming, "Nothing would be left to end aiming with the button hidden");
        Assert.IsFalse(_aimHighlight.activeSelf, "The button shouldn't show aiming");
    }

    #endregion

    #region Pause

    [Test]
    public void Should_Show_Pause_State_On_Pause_Icon()
    {
        var icon = getField<Image>(_touchControls, "_pauseIcon");

        _gameManager.SwitchPauseState();
        Assert.AreEqual(getField<Sprite>(_touchControls, "_resumeSprite"), icon.sprite, "A paused game should offer to resume");

        _gameManager.SwitchPauseState();
        Assert.AreEqual(getField<Sprite>(_touchControls, "_pauseSprite"), icon.sprite, "A running game should offer to pause");
    }

    [Test]
    public void Should_Switch_Pause_On_Timer_Tap()
    {
        Assert.IsNotNull(getField<RectTransform>(_touchControls.LookArea, "_tapZone"), "The timer should have its tap zone");

        raise(_touchControls.LookArea, "ZoneTapped");
        Assert.IsTrue(_gameManager.GamePaused, "A tap should pause");

        raise(_touchControls.LookArea, "ZoneTapped");
        Assert.IsFalse(_gameManager.GamePaused, "Another tap should resume");
    }

    [Test]
    public void Should_Pause_On_Rotation_And_Stay_Paused_When_Already_Paused()
    {
        var sceneManager = Object.FindAnyObjectByType<SceneManager>();

        sceneManager.PauseOnRotation();
        Assert.IsTrue(_gameManager.GamePaused, "Turning the device should pause a running game");

        sceneManager.PauseOnRotation();
        Assert.IsTrue(_gameManager.GamePaused, "Turning the device shouldn't resume a paused game");
    }

    [Test]
    public void Should_Resume_On_Pause_Message_Tap_On_Touch_Only()
    {
        var pauseTab = Object.FindAnyObjectByType<TouchPauseTab>(FindObjectsInactive.Include);
        Assert.IsNotNull(pauseTab, "The pause message should be a button");
        _gameManager.SwitchPauseState();

        pauseTab.OnPointerClick(makeEvent());
        Assert.IsTrue(_gameManager.GamePaused, "A click off touch shouldn't resume");

        setField(pauseTab, "_touchMode", new TouchMode(isSupported: true, isActive: true));
        pauseTab.OnPointerClick(makeEvent());
        Assert.IsFalse(_gameManager.GamePaused, "A tap should resume");

        pauseTab.OnPointerClick(makeEvent());
        Assert.IsFalse(_gameManager.GamePaused, "A tap on a running game shouldn't pause it");
    }

    #endregion

    #region Layout

    [Test]
    public void Should_Keep_Aim_Button_Out_Of_Stick_Zone_On_Narrow_Screen()
    {
        Assume.That(Screen.height, Is.GreaterThan(0), "It takes a screen to measure");
        var aim = (RectTransform)_touchControls.Aim.transform;
        Vector2 wide = aim.anchoredPosition;

        foreach (float aspect in new[] {4f / 3, 1})
        {
            fitRightControls(aspect);
            float width = aspect * 1080;
            float leftEdge = width + aim.anchoredPosition.x - aim.rect.width * aim.localScale.x / 2;
            Assert.GreaterOrEqual(leftEdge, width * 0.4f - 0.01f, $"The button should clear the stick's zone at {aspect}");
        }

        fitRightControls(aspect: 16f / 9);
        Assert.AreEqual(wide, aim.anchoredPosition, "A wide screen should bring it back");
        Assert.AreEqual(Vector3.one, aim.localScale, "A wide screen should keep its full size");
    }

    [Test]
    public void Should_Stay_Shrunk_After_A_Press()
    {
        Assume.That(Screen.height, Is.GreaterThan(0), "It takes a screen to measure");
        Transform aim = _touchControls.Aim.transform;

        fitRightControls(aspect: 1);
        Vector3 shrunk = aim.localScale;
        Assert.Less(shrunk.x, 1, "A square screen should shrink the button");

        _touchControls.Aim.OnPointerDown(makeEvent());
        Assert.Less(aim.localScale.x, shrunk.x, "A press should still show");
        _touchControls.Aim.OnPointerUp(makeEvent());
        Assert.AreEqual(shrunk, aim.localScale, "Letting go shouldn't bring back the full size");
    }

    #endregion

    #region Helpers

    private void pressAim()
    {
        _touchControls.Aim.OnPointerDown(makeEvent());
        _touchControls.Aim.OnPointerUp(makeEvent());
    }

    private void fitRightControls(float aspect)
    {
        setField(_touchControls, "_appliedSafeArea", new Rect(0, 0, Screen.height * aspect, Screen.height));
        typeof(TouchControls).GetMethod("fitRightControls", _private).Invoke(_touchControls, null);
    }

    private PointerEventData makeEvent() => new(null) { pointerId = _finger };

    private T getField<T>(object target, string name) => (T)target.GetType().GetField(name, _private).GetValue(target);

    private void setField(object target, string name, object value) => target.GetType().GetField(name, _private).SetValue(target, value);

    // An event's own field holds its handlers
    private void raise(object target, string name, params object[] args)
    {
        var handlers = (System.Delegate)target.GetType().GetField(name, _private).GetValue(target);
        handlers.DynamicInvoke(args);
    }

    #endregion
}
