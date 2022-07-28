using System.Collections.Generic;
using NUnit.Framework;

using DamageModifications = System.Collections.Generic.Dictionary<DamageType, float>;


public class OneWaveDamageModifierTests
{
    [Test]
    public void Should_Have_Default_Modicitaions_As_Initial()
    {
        OneWaveDamageModifier damageModifier = new();
        DamageModifications expected = GameUtils.DefaultModifications();
        DamageModifications actual = damageModifier.GetModifications();

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void Should_Reset_Modicitaions_To_Default()
    {
        OneWaveDamageModifier damageModifier = new();
        DamageModifications expected = damageModifier.GetModifications();
        PlayerKillsCollector killsCollector = new();

        killsCollector.AddValue(DamageType.FIST);
        damageModifier.AnalyzeWaveStats(killsCollector);
        damageModifier.ResetModifications();
        DamageModifications actual = damageModifier.GetModifications();

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void Should_Set_Modifications_Within_Range_From_0_to_1()
    {
        OneWaveDamageModifier damageModifier = new();
        PlayerKillsCollector killsCollector = new();
        int min = 0;
        int max = 1;

        killsCollector.AddValue(DamageType.FIST);
        damageModifier.AnalyzeWaveStats(killsCollector);
        DamageModifications modifications = damageModifier.GetModifications();

        foreach (var modification in modifications)
        {
            DamageType key = modification.Key;
            float value = modification.Value;
            Assert.Greater(value, min, message: $"Value {value} ({key}) isn't greater than {min}");
            Assert.LessOrEqual(value, max, message: $"Value {value} ({key}) isn't lesser than or equal to {max}");
        }
    }

    [Test]
    public void Should_Set_100_Percent_If_Collector_Value_Is_Zero()
    {
        OneWaveDamageModifier damageModifier = new();
        PlayerKillsCollector killsCollector = new();
        int expected = 1;

        // non-zero modifications
        killsCollector.AddValue(DamageType.FIST);
        killsCollector.AddValue(DamageType.GRENADE);
        damageModifier.AnalyzeWaveStats(killsCollector);
        DamageModifications modifications = damageModifier.GetModifications();

        Assert.AreEqual(expected, modifications[DamageType.MACHETE], message: $"Value {modifications[DamageType.MACHETE]} ({DamageType.MACHETE}) should be equal to {expected}");
        Assert.AreEqual(expected, modifications[DamageType.ARROW], message: $"Value {modifications[DamageType.ARROW]} ({DamageType.ARROW}) should be equal to {expected}");
        Assert.AreNotEqual(expected, modifications[DamageType.FIST], message: $"Value {modifications[DamageType.FIST]} ({DamageType.FIST}) cannot be equal to {expected}");
        Assert.AreNotEqual(expected, modifications[DamageType.GRENADE], message: $"Value {modifications[DamageType.GRENADE]} ({DamageType.GRENADE}) cannot be equal to {expected}");
    }

    [Test]
    public void Should_Set_10_Percent_If_Collector_Value_Is_Max()
    {
        OneWaveDamageModifier damageModifier = new();
        PlayerKillsCollector killsCollector = new();
        float expected = 0.1f;

        // max and only collector value
        killsCollector.AddValue(DamageType.FIST);
        damageModifier.AnalyzeWaveStats(killsCollector);
        DamageModifications modifications = damageModifier.GetModifications();

        Assert.AreEqual(expected, modifications[DamageType.FIST], message: $"Value {modifications[DamageType.FIST]} ({DamageType.FIST}) should be equal to {expected}");
        Assert.AreNotEqual(expected, modifications[DamageType.MACHETE], message: $"Value {modifications[DamageType.MACHETE]} ({DamageType.MACHETE}) cannot be equal to {expected}");
        Assert.AreNotEqual(expected, modifications[DamageType.ARROW], message: $"Value {modifications[DamageType.ARROW]} ({DamageType.ARROW}) cannot be equal to {expected}");
        Assert.AreNotEqual(expected, modifications[DamageType.GRENADE],  message: $"Value {modifications[DamageType.GRENADE]} ({DamageType.GRENADE}) cannot be equal to {expected}");
    }

    [Test]
    public void Should_Set_Correct_Precentage_Based_On_Collector_Values()
    {
        OneWaveDamageModifier damageModifier = new();
        PlayerKillsCollector killsCollector = new();
        int total = 20;
        Dictionary<DamageType, int> kills = new() {
            {DamageType.FIST, 5},
            {DamageType.MACHETE, 4},
            {DamageType.ARROW, 10},
            {DamageType.GRENADE, 1},
        };

        foreach (var kill in kills)
            for(int i = 0; i < kill.Value; ++i) killsCollector.AddValue(kill.Key);
        
        damageModifier.AnalyzeWaveStats(killsCollector);
        DamageModifications modifications = damageModifier.GetModifications();
        
        foreach (var modification in modifications)
        {
            DamageType key = modification.Key;
            float expected = (float)(total - kills[key]) / total;
            float actual = modification.Value;
            Assert.AreEqual(expected, actual, message: $"Value {expected} ({key}) should be equal to {actual}");
        }
    }
}
