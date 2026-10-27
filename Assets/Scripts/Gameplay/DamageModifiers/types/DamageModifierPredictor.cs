using System;
using System.Collections.Generic;
using System.Linq;
using Unity.InferenceEngine;

public class DamageModifierPredictor : IDamageModifier, IDisposable
{
    public static DamageModifierPredictor FromModel(ModelAsset model) => new(InferencePredictionWorker.FromModel(model));

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

    private readonly IPredictionWorker _worker;

    public DamageModifierPredictor(IPredictionWorker worker)
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
        var input = new float[inputLength];
        for (int i = 0; i < inputLength; ++i) input[i] = inputParams[_inputOrder[i]];
        _worker.Execute(input);
        foreach (var label in _modelLabels)
        {
            float output = _worker.PeekOutput(label.Value).Max();
            output /= 100;
            _modifications[label.Key] = GameUtils.FormatModifierValue(output);
        }
    }

    private float adaptInputParam(List<float> data)
    {
        float stdev = GameUtils.Stdev(data);
        // Unchanged share across waves has no deviation, so it is neutral instead of 0/0
        if (stdev == 0) return 0;
        return (data.Last() - GameUtils.Mean(data)) / stdev;
    }

    private float roundPercentage(float value) => MathF.Round(value, digits: 2);
}
