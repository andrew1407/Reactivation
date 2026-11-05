using Unity.InferenceEngine;

public class InferencePredictionWorker : IPredictionWorker
{
    public static InferencePredictionWorker FromModel(ModelAsset modelAsset)
    {
        Model runtimeModel = ModelLoader.Load(modelAsset);
        // The web player runs on WebGPU or falls back to WebGL 2, which has no compute shaders, and compute support
        // varies a lot between phones: the model is tiny, so it runs on the CPU there either way
        bool shouldUseGpu = UnityEngine.SystemInfo.supportsComputeShaders
            && UnityEngine.Application.platform != UnityEngine.RuntimePlatform.WebGLPlayer
            && !UnityEngine.Application.isMobilePlatform;
        DeviceType device = shouldUseGpu ? DeviceType.GPU : DeviceType.CPU;
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

    public float[] PeekOutput(string name) => ((Tensor<float>)_worker.PeekOutput(name)).DownloadToArray();

    public void Dispose()
    {
        _input?.Dispose();
        _worker.Dispose();
    }
}
