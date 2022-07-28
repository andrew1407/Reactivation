using NUnit.Framework;

using DamageModifications = System.Collections.Generic.Dictionary<DamageType, float>;

public class RandomDamageModifierTests
{
    [Test]
    public void Should_Have_Default_Modicitaions_As_Initial()
    {
        RandomDamageModifier damageModifier = new();
        DamageModifications expected = GameUtils.DefaultModifications();
        DamageModifications actual = damageModifier.GetModifications();

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void Should_Reset_Modicitaions_To_Default()
    {
        RandomDamageModifier damageModifier = new();
        DamageModifications expected = damageModifier.GetModifications();
        
        damageModifier.AnalyzeWaveStats(null);
        damageModifier.ResetModifications();
        DamageModifications actual = damageModifier.GetModifications();

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void Should_Set_Modifications_Within_Range_From_0_to_1()
    {
        RandomDamageModifier damageModifier = new();
        int min = 0;
        int max = 1;

        damageModifier.AnalyzeWaveStats(null);
        DamageModifications modifications = damageModifier.GetModifications();

        foreach (var modification in modifications)
        {
            DamageType key = modification.Key;
            float value = modification.Value;
            Assert.Greater(value, min, message: $"Value {value} ({key}) isn't greater than {min})");
            Assert.LessOrEqual(value, max, message: $"Value {value} ({key}) isn't lesser than or equal to {max}");
        }
    }
}
