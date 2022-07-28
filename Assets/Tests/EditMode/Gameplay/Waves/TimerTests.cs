using NUnit.Framework;
using UnityEngine;

public class TimerTests
{
    [Test]
    public void Should_Be_Inaactive_By_Default()
    {
        uint initialSeconds = 7;
        Timer timer = new(initialSeconds);

        timer.OnTick();

        Assert.IsFalse(timer.Active, "Timer should be inactive by default");
        Assert.AreEqual(initialSeconds, timer.CurrentTime, "Curent time cannot change if timer is inactive");
    }

    [Test]
    public void Should_Countdown_Substracting_Delta_Time()
    {
        uint initialSeconds = 7;
        Timer timer = new(initialSeconds);
        int ticks = 3;
        float expectedSeconds = initialSeconds - Time.deltaTime * ticks;
        float delta = 0.1f;

        timer.Active = true;
        for (int i = 0; i < ticks; ++i) timer.OnTick();

        Assert.AreEqual(expectedSeconds, timer.CurrentTime, delta);
    }

    [Test]
    public void Should_Not_Countdown_After_Timer_Set_Inactive()
    {
        uint initialSeconds = 7;
        Timer timer = new(initialSeconds);
        int ticksActive = 3;
        int ticksInacive = 6;
        float expectedSeconds = initialSeconds - Time.deltaTime * ticksActive;
        float delta = 0.1f;

        timer.Active = true;
        for (int i = 0; i < ticksInacive; ++i)
        {
            if (i == ticksActive && timer.Active) timer.Active = false;
            timer.OnTick();
        }

        Assert.AreEqual(expectedSeconds, timer.CurrentTime, delta);
    }

    [Test]
    public void Should_Set_Zero_As_Lower_Bound()
    {
        uint initialSeconds = 0;
        Timer timer = new(initialSeconds);

        timer.Active = true;
        timer.OnTick();
        timer.OnTick();
        timer.OnTick();

        Assert.Zero(timer.CurrentTime);
    }

    [Test]
    public void Should_Reset_Timer_Without_Stopping()
    {
        uint initialSeconds = 7;
        Timer timer = new(initialSeconds);

        timer.Active = true;
        timer.OnTick();
        timer.OnTick();
        timer.OnTick();
        timer.Reset();

        Assert.IsTrue(timer.Active, message: "Timer should remain active after reset");
        Assert.AreEqual(initialSeconds, timer.CurrentTime, message: "Timer should reset current time as initial");
    }

    [Test]
    public void Should_Not_Activate_Timer_On_Reset()
    {
        uint initialSeconds = 7;
        Timer timer = new(initialSeconds);

        timer.Reset();

        Assert.IsFalse(timer.Active);
    }
}
