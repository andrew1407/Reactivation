using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class StuckAmmoTests
{
    private GameObject _enemy;

    private Arrow _arrow;

    private GameObject _arrowView;

    [SetUp]
    public void SetUpArrowInEnemy()
    {
        _enemy = new GameObject("Enemy");
        _enemy.AddComponent<Enemy>();
        _arrow = new GameObject("Arrow").AddComponent<Arrow>();
        _arrowView = new GameObject("View");
        _arrowView.transform.SetParent(_arrow.transform);
        typeof(PlayerAmmo).GetField("_ammoView", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_arrow, _arrowView);
        _arrow.transform.SetParent(_enemy.transform);
    }

    [TearDown]
    public void TearDownObjects()
    {
        Object.DestroyImmediate(_arrow.gameObject);
        Object.DestroyImmediate(_enemy);
    }

    [Test]
    public void Should_Release_Stuck_Ammo_When_Enemy_Dies()
    {
        _enemy.GetComponent<Enemy>().Die(deathEffect: false);

        Assert.IsNull(_arrow.transform.parent, "The arrow mustn't stay inside the deactivated enemy");
        Assert.IsTrue(_arrow.gameObject.activeInHierarchy, "The arrow must stay active to count down its return to the quiver");
        Assert.IsFalse(_arrowView.activeSelf, "The arrow should disappear together with the enemy");
    }

    [Test]
    public void Should_Keep_Ammo_Position_When_Released()
    {
        _enemy.transform.position = new Vector3(3, 0, 5);
        _arrow.transform.localPosition = new Vector3(0, 1, 0);

        _enemy.GetComponent<Enemy>().Die(deathEffect: false);

        Assert.AreEqual(new Vector3(3, 1, 5), _arrow.transform.position);
    }
}
