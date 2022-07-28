using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Barracuda;

public class DamageModifierPredictor : IDamageModifier, IDisposable
{
    public static DamageModifierPredictor FromModel(NNModel model)
    {
        Model runtimeModel = ModelLoader.Load(model);
        IWorker worker = WorkerFactory.CreateWorker(runtimeModel, WorkerFactory.Device.GPU);
        return new DamageModifierPredictor(worker);
    }

    private readonly Dictionary<DamageType, string> _modelLabels = new() {
        {DamageType.FIST, "fist"},
        {DamageType.MACHETE, "machete"},
        {DamageType.ARROW, "arrow"},
        {DamageType.GRENADE, "grenade"},
    };

    private readonly DamageType[] _inputOrder = new[] {
        DamageType.FIST,
        DamageType.MACHETE,
        DamageType.ARROW,
        DamageType.GRENADE,
    };

    private readonly Dictionary<DamageType, float> _modifications;

    private readonly Dictionary<DamageType, List<float>> _waveStats = new();

    private readonly IWorker _worker;

    public DamageModifierPredictor(IWorker worker)
    {
        _worker = worker;
        _modifications = GameUtils.DefaultModifications();
        setInitialStats();
    }

    public Dictionary<DamageType, float> GetModifications() => new(_modifications);

    public void AnalyzeWaveStats(ICollector collector)
    {
        var predictInput = addNewStats(collector.GetStats());
        setPredictedModifiactions(predictInput);
    }

    public void ResetModifications()
    {
        foreach (var defaultModifier in GameUtils.DefaultModifications())
        {
            _modifications[defaultModifier.Key] = defaultModifier.Value;
            var stats = _waveStats[defaultModifier.Key];
            if (stats.Count > 1) stats.RemoveRange(1, stats.Count - 1);
        }
    }

    public void Dispose() => _worker?.Dispose();

    private void setInitialStats()
    {
        var keys = _modifications.Keys.ToList();
        float initialPercentage = roundPercentage(100f / (float)keys.Count);
        foreach (var key in keys) _waveStats[key] = new List<float> { initialPercentage };
    }

    private Dictionary<DamageType, float> addNewStats(Dictionary<DamageType, int> stats)
    {
        float total = stats.Values.Sum();
        Dictionary<DamageType, float> predictInput = new();
        foreach (var stat in _waveStats)
        {
            var metrics = stat.Value;
            var key = stat.Key;
            float percentage = (float)stats[key] / total * 100;
            metrics.Add(roundPercentage(percentage));
            predictInput[key] = roundPercentage(adaptInputParam(metrics));
        }
        return predictInput;
    }

    private void setPredictedModifiactions(Dictionary<DamageType, float> inputParams)
    {
        int inputLength = _inputOrder.Length;
        using var input = new Tensor(1, inputLength);
        for (int i = 0; i < inputLength; ++i) input[i] = inputParams[_inputOrder[i]];
        _worker.Execute(input);
        foreach (var label in _modelLabels)
        {
            Tensor outputs = _worker.PeekOutput(label.Value);
            float output = outputs.AsFloats().Max();
            output /= 100;
            _modifications[label.Key] = GameUtils.FormatModifierValue(output);
        }
    }

    private float adaptInputParam(List<float> data) => (data.Last() - GameUtils.Mean(data)) / GameUtils.Stdev(data);

    private float roundPercentage(float value) => MathF.Round(value, digits: 2);
}
