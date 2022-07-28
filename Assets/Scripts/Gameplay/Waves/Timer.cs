using UnityEngine;

public class Timer
{
    private readonly uint _countdownSeconds;

    public bool Active = false;

    public float CurrentTime { get; private set; }

    public Timer(uint seconds)
    {
        CurrentTime = _countdownSeconds = seconds;
    }

    public void OnTick()
    {
        if (Active) CurrentTime = Mathf.Max(CurrentTime - Time.deltaTime, 0);
    }

    public void Reset()
    {
        CurrentTime = _countdownSeconds;
    }
}
