using System;
using UnityEngine;
using UnityEngine.Animations.Rigging;

[Serializable]
public struct CharacterComponents
{
    public Animator BodyAnimator;

    public Animator RigAnimator;

    public Transform Transform;

    public RigBuilder RigBuilder;

    public int Layer;
}
