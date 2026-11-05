using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// Boots the real level: any exception or error logged while the scene's installers, spawners
// and the damage predictor start up fails the test
public class MainSceneSmokeTests
{
    private const string _mainScene = "MainScene";

    private const float _spawnTimeoutSeconds = 10;

    [UnityTest]
    public IEnumerator Should_Load_Main_Scene_And_Spawn_Player()
    {
        yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(_mainScene);

        Player player = null;
        float waited = 0;
        while (player == null && waited < _spawnTimeoutSeconds)
        {
            yield return null;
            waited += Time.unscaledDeltaTime;
            player = Object.FindAnyObjectByType<Player>();
        }

        Assert.IsNotNull(player, $"The player wasn't spawned within {_spawnTimeoutSeconds}s of loading {_mainScene}");
        Assert.IsTrue(player.gameObject.activeInHierarchy, "The spawned player should be active");

        // A few more frames of the running level
        for (int i = 0; i < 30; ++i) yield return null;

        var touchControls = Object.FindAnyObjectByType<TouchControls>();
        Assert.IsNotNull(touchControls, "The touch controls should be in the level");
        Assert.IsFalse(touchControls.MoveStick.gameObject.activeInHierarchy, "A desktop game shouldn't show them");
        Assert.AreEqual(Vector3.one, Object.FindAnyObjectByType<TouchEnlarged>().transform.localScale, "A desktop game shouldn't enlarge its text");
    }
}
