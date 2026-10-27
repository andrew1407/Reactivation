using System;

public interface IPredictionWorker : IDisposable
{
    void Execute(float[] input);

    float[] PeekOutput(string name);
}
