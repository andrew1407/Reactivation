using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// Boots the real level: any exception or error logged while the scene's installers, spawners
// and the damage predictor start up fails the test
public class MainSceneSmokeTests
{
    private const string MainScene = "MainScene";

    private const float SpawnTimeoutSeconds = 10;

    [UnityTest]
    public IEnumerator Should_Load_Main_Scene_And_Spawn_Player()
    {
        yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(MainScene);

        Player player = null;
        float waited = 0;
        while (player == null && waited < SpawnTimeoutSeconds)
        {
            yield return null;
            waited += Time.unscaledDeltaTime;
            player = Object.FindAnyObjectByType<Player>();
        }

        Assert.IsNotNull(player, $"The player wasn't spawned within {SpawnTimeoutSeconds}s of loading {MainScene}");
        Assert.IsTrue(player.gameObject.activeInHierarchy, "The spawned player should be active");

        // A few more frames of the running level
        for (int i = 0; i < 30; ++i) yield return null;
    }
}
