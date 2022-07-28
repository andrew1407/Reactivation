using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Barracuda;

using DamageModifications = System.Collections.Generic.Dictionary<DamageType, float>;

public class DamageModifierPredictorTests
{
   [Test]
    public void Should_Have_Default_Modicitaions_As_Initial()
    {
        DamageModifierPredictor damageModifier = new(new WorkerMock());
        DamageModifications expected = GameUtils.DefaultModifications();
        DamageModifications actual = damageModifier.GetModifications();

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void Should_Reset_Modicitaions_To_Default()
    {
        DamageModifierPredictor damageModifier = new(new WorkerMock());
        PlayerKillsCollector killsCollector = new();
        DamageModifications expected = damageModifier.GetModifications();

        damageModifier.AnalyzeWaveStats(killsCollector);
        damageModifier.ResetModifications();
        DamageModifications actual = damageModifier.GetModifications();

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void Should_Set_Modifications_Within_Range_From_0_to_1()
    {
        DamageModifierPredictor damageModifier = new(new WorkerMock());
        PlayerKillsCollector killsCollector = new();
        int min = 0;
        int max = 1;

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
    public void Should_Set_1_Percent_To_All_Modifications_As_Lower_Bound()
    {
        DamageModifierPredictor damageModifier = new(new WorkerMock());
        PlayerKillsCollector killsCollector = new();
        float expected = 0.01f;

        damageModifier.AnalyzeWaveStats(killsCollector);
        DamageModifications modifications = damageModifier.GetModifications();

        foreach (var modification in modifications)
        {
            DamageType key = modification.Key;
            float value = modification.Value;
            Assert.AreEqual(expected, value, message: $"Value {value} ({key}) should be equal {expected}");
        }
    }

    [Test]
    public void Should_Execute_And_Peek_Output_From_Worker_In_Analyze_Method()
    {
        WorkerMock workerMock = new();
        DamageModifierPredictor damageModifier = new(workerMock);
        PlayerKillsCollector killsCollector = new();
        int executeCallsExpected = 1;
        int peekOutputCallsExpected = 4;

        damageModifier.AnalyzeWaveStats(killsCollector);

        Assert.AreEqual(executeCallsExpected, workerMock.ExecuteCalled, message: $"Worker's method .execute() wasn't called {executeCallsExpected} times");
        Assert.AreEqual(peekOutputCallsExpected, workerMock.PeekOutputCalled, message: $"Worker's method .peekOutput() wasn't called {peekOutputCallsExpected} times");
    }

    [Test]
    public void Should_Pass_Kills_Percentage_As_Tensor_Parameters_At_Analyze()
    {
        WorkerMock workerMock = new();
        DamageModifierPredictor damageModifier = new(workerMock);
        PlayerKillsCollector killsCollector = new();
        Dictionary<DamageType, float> tensorValues = new() {
            {DamageType.FIST, 0},
            {DamageType.MACHETE, 0},
            {DamageType.ARROW, 0},
            {DamageType.GRENADE, 0},
        };
        Dictionary<DamageType, int> kills = new() {
            {DamageType.FIST, 5},
            {DamageType.MACHETE, 4},
            {DamageType.ARROW, 10},
            {DamageType.GRENADE, 1},
        };

        foreach (var kill in kills)
            for(int i = 0; i < kill.Value; ++i) killsCollector.AddValue(kill.Key);
        
        damageModifier.AnalyzeWaveStats(killsCollector);

        Tensor resultTensor = workerMock.InputTensor;
        Assert.AreEqual(tensorValues.Count, resultTensor.length, message: "Invalid tensor length");

        Assert.AreEqual(tensorValues[DamageType.FIST], resultTensor[0], message: $"The result tensor value is invalid for {DamageType.FIST}");
        Assert.AreEqual(tensorValues[DamageType.MACHETE], resultTensor[1], message: $"The result tensor value is invalid for {DamageType.MACHETE}");
        Assert.AreEqual(tensorValues[DamageType.ARROW], resultTensor[2], message: $"The result tensor value is invalid for {DamageType.ARROW}");
        Assert.AreEqual(tensorValues[DamageType.GRENADE], resultTensor[3], message: $"The result tensor value is invalid for {DamageType.GRENADE}");
    }

    [Test]
    public void Should_Parse_Tensor_Output_Persentage()
    {
        WorkerMock workerMock = new();
        DamageModifierPredictor damageModifier = new(workerMock);
        PlayerKillsCollector killsCollector = new();
        DamageModifications tensorOutput = new() {
            {DamageType.FIST, 50},
            {DamageType.MACHETE, 30},
            {DamageType.ARROW, 12.5f},
            {DamageType.GRENADE, 74.4467f},
        };
        Func<float, float> round = n => MathF.Round(n / 100, digits: 2);

        workerMock.OutputTensors["fist"] = new();
        workerMock.OutputTensors["fist"][0] = tensorOutput[DamageType.FIST];

        workerMock.OutputTensors["machete"] = new();
        workerMock.OutputTensors["machete"][0] = tensorOutput[DamageType.MACHETE];

        workerMock.OutputTensors["arrow"] = new();
        workerMock.OutputTensors["arrow"][0] = tensorOutput[DamageType.ARROW];

        workerMock.OutputTensors["grenade"] = new();
        workerMock.OutputTensors["grenade"][0] = tensorOutput[DamageType.GRENADE];


        damageModifier.AnalyzeWaveStats(killsCollector);
        DamageModifications modifications = damageModifier.GetModifications();


        Assert.AreEqual(expected: round(tensorOutput[DamageType.FIST]), actual: modifications[DamageType.FIST], message: $"Invalid modifier value for {DamageType.FIST}");
        Assert.AreEqual(expected: round(tensorOutput[DamageType.MACHETE]), actual: modifications[DamageType.MACHETE], message: $"Invalid modifier value for {DamageType.MACHETE}");
        Assert.AreEqual(expected: round(tensorOutput[DamageType.ARROW]), actual: modifications[DamageType.ARROW], message: $"Invalid modifier value for {DamageType.ARROW}");
        Assert.AreEqual(expected: round(tensorOutput[DamageType.GRENADE]), actual: modifications[DamageType.GRENADE], message: $"Invalid modifier value for {DamageType.GRENADE}");
    }

    [Test]
    public void Should_Dispose_Worker_Once()
    {
        WorkerMock workerMock = new();
        DamageModifierPredictor damageModifier = new(workerMock);
        int expected = 1;

        damageModifier.Dispose();

        Assert.AreEqual(expected, workerMock.DisposeCalled, message: $"Worker's method .execute() wasn't called {expected} times");
    }

    private class WorkerMock : IWorker
    {
        public float scheduleProgress { get; }

        public int ExecuteCalled { get; private set; }

        public int PeekOutputCalled { get; private set; }

        public int DisposeCalled { get; private set; }

        public Tensor InputTensor { get; private set; }

        public Dictionary<string, Tensor> OutputTensors = new();
        
        public IWorker Execute(Tensor input)
        {
            InputTensor = input;
            ++ExecuteCalled;
            return this;
        }

        public Tensor PeekOutput(string name)
        {
            ++PeekOutputCalled;
            if (!OutputTensors.ContainsKey(name)) return new();
            return OutputTensors[name];
        }

        public void Dispose() => ++DisposeCalled;

        public IWorker Execute() => null;
        public IWorker Execute(IDictionary<string, Tensor> inputs) => null;
        public void FlushSchedule(bool blocking = false) {}
        public Tensor[] PeekConstants(string layerName) => Array.Empty<Tensor>();
        public Tensor PeekOutput() => null;
        public void PrepareForInput(IDictionary<string, TensorShape> inputShapes) {}
        public void SetInput(Tensor x) {}
        public void SetInput(string name, Tensor x) {}
        public IEnumerator StartManualSchedule() => null;
        public IEnumerator StartManualSchedule(Tensor input) => null;
        public IEnumerator StartManualSchedule(IDictionary<string, Tensor> inputs) => null;
        public string Summary() => string.Empty;
    }
}
