using UnityEngine;
using UnityEngine.Events;
using Zenject;

public class GrenadeAttackAnimation : MonoBehaviour
{
    [Inject] private readonly GrenadeHolder _grenadeHolder;

    public readonly UnityEvent OnThrow = new();

    private readonly string _throwEvent = "was_thrown";

    public void OnGrenadeAttackAnimation(string eventName)
    {
        if (_throwEvent != eventName) return;
        _grenadeHolder.OnAnimationTrigger();
    }
}
