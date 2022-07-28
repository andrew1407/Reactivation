using System.Collections;
using UnityEngine;
using Zenject;

public abstract class SteelArmsWeapon : IWeapon
{
    private readonly int _idleHash = Animator.StringToHash("idle");
    
    private readonly int _animationHash;

    private Animator _animator;

    public string Label { get; private set; }

    public SteelArmsWeapon(string label, string actionName)
    {
        Label = label;
        _animationHash = Animator.StringToHash(actionName);
    }

    [Inject]
    private void Construct(CharacterComponents characterComponents)
    {
        _animator = characterComponents.RigAnimator;
    }

    public IEnumerator Action()
    {
        yield return GameUtils.PlayAnimation(_animator, _animationHash);
        _animator.Play(_idleHash);
        yield return null;
    }
}
