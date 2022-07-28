using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

using StatsContainer = System.Collections.Generic. Dictionary<DamageType, int>;

public class PlayerKillsCollectorTests
{
    [Test]
    public void Should_Set_Zeros_As_Default_Value()
    {
        PlayerKillsCollector killsCollector = new();
        StatsContainer stats = killsCollector.GetStats();

        foreach(var stat in stats)
            Assert.Zero(stat.Value, message: $"Default {stat.Key} value isn't emplty");
    }

    [Test]
    public void Should_Add_Values_Correctly_With_Multiple_Threads()
    {
        List<Thread> threads = new();
        Action<Action[]> start = actions =>
        {
            foreach (var fn in actions)
            {
                var t = new Thread(() => fn());
                t.Start();
                threads.Add(t);
            }
        };
        Action waitFinish = () =>
        {
            foreach (var t in threads) t.Join();
        };

        testConcurrency(start, waitFinish);
    }
    
    [Test]
    public void Should_Add_Values_Correctly_With_Multiple_Tasks()
    {
        List<Task> tasks = new();
        Action<Action[]> start = actions =>
        {
            foreach (var fn in actions) tasks.Add(Task.Run(fn));
        };
        Action waitFinish = () => Task.WaitAll(tasks.ToArray());

        testConcurrency(start, waitFinish);
    }

    [Test]
    public void Should_Clear_Collected_Data()
    {
        PlayerKillsCollector killsCollector = new();

        killsCollector.AddValue(DamageType.FIST);
        killsCollector.AddValue(DamageType.MACHETE);
        killsCollector.AddValue(DamageType.MACHETE);
        killsCollector.ClearData();

        StatsContainer sStats = killsCollector.GetStats();

        foreach(var stat in sStats)
            Assert.Zero(stat.Value, message: $"Default {stat.Key} value wasn't cleared");
    }

    private void testConcurrency(Action<Action[]> start, Action waitFinish)
    {
        PlayerKillsCollector killsCollector = new();
        StatsContainer expectedValues = new() {
            {DamageType.FIST, 6},
            {DamageType.MACHETE, 10},
            {DamageType.ARROW, 5},
            {DamageType.GRENADE, 8},
        };
        Action<DamageType, int> addValues = (damageType, count) =>
        {
            for (int i = 0; i < count; ++i) killsCollector.AddValue(damageType);
        };

        start(new Action[] {
            () => addValues(DamageType.MACHETE, 4),
            () => addValues(DamageType.FIST, 4),
            () => addValues(DamageType.MACHETE, 6),
        });

        addValues(DamageType.FIST, 2);
        addValues(DamageType.ARROW, 5);
        addValues(DamageType.GRENADE, 8);

        waitFinish();

        StatsContainer stats = killsCollector.GetStats();

        foreach (var expected in expectedValues)
        {
            DamageType key = expected.Key;
            int expectedValue = expected.Value;
            float actual = stats[key];
            Assert.AreEqual(expectedValue, actual, message: $"Calcuated stats value is invalid for {key}");
        }
    }
}
