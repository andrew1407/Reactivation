using Unity.InferenceEngine;
using UnityEngine;

public class InferencePredictionWorker : IPredictionWorker
{
    public static InferencePredictionWorker FromModel(ModelAsset modelAsset)
    {
        Model runtimeModel = ModelLoader.Load(modelAsset);
        // WebGL has no compute shaders, so the GPU backend is unavailable there
        DeviceType device = SystemInfo.supportsComputeShaders ? DeviceType.GPU : DeviceType.CPU;
        Worker worker = new(runtimeModel, device);
        return new InferencePredictionWorker(worker);
    }

    private readonly Worker _worker;

    // Kept alive until the next execution: the scheduled work may still read from it
    private Tensor<float> _input;

    public InferencePredictionWorker(Worker worker) => _worker = worker;

    public void Execute(float[] input)
    {
        _input?.Dispose();
        _input = new Tensor<float>(new TensorShape(1, input.Length), input);
        _worker.Schedule(_input);
    }

    public float[] PeekOutput(string name) => (_worker.PeekOutput(name) as Tensor<float>).DownloadToArray();

    public void Dispose()
    {
        _input?.Dispose();
        _worker.Dispose();
    }
}
