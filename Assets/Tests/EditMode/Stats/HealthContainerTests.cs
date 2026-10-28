using NUnit.Framework;

public class HealthContainerTests
{
    [Test]
    public void Should_Start_With_Max_Health()
    {
        HealthContainer health = new(100);

        Assert.AreEqual(100, health.HealthPoints);
    }

    [Test]
    public void Should_Modify_Points()
    {
        HealthContainer health = new(100);

        health.ModifyPoints(-30);
        health.ModifyPoints(10);

        Assert.AreEqual(80, health.HealthPoints);
    }

    [Test]
    public void Should_Set_Zero_As_Lower_Bound()
    {
        HealthContainer health = new(100);

        health.ModifyPoints(-250);

        Assert.AreEqual(0, health.HealthPoints);
    }

    [Test]
    public void Should_Set_Max_Health_As_Upper_Bound()
    {
        HealthContainer health = new(100);

        health.ModifyPoints(-10);
        health.ModifyPoints(50);

        Assert.AreEqual(100, health.HealthPoints);
    }

    [Test]
    public void Should_Truncate_Fractional_Points()
    {
        HealthContainer health = new(100);

        health.ModifyPoints(-0.5f);

        Assert.AreEqual(99, health.HealthPoints, "Partly lost points should already show as lost");
    }

    [Test]
    public void Should_Accumulate_Fractional_Damage()
    {
        HealthContainer health = new(100);

        for (int i = 0; i < 4; ++i) health.ModifyPoints(-0.5f);

        Assert.AreEqual(98, health.HealthPoints);
    }

    [Test]
    public void Should_Restore_Max_Health()
    {
        HealthContainer health = new(100);

        health.ModifyPoints(-70);
        health.SetMaxHealth();

        Assert.AreEqual(100, health.HealthPoints);
    }
}
