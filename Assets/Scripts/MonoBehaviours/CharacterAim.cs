using UnityEngine;
using UnityEngine.Animations.Rigging;
using Zenject;

public class CharacterAim : MonoBehaviour
{
    [SerializeField] private MultiAimConstraint[] _multiAimConstraints;

    [Inject] private readonly CharacterComponents _characterComponents;

    public void SetAim(Transform aim)
    {
        RigBuilder rigBuilder = _characterComponents.RigBuilder;
        foreach (var constraint in _multiAimConstraints)
        {
            var transforms = constraint.data.sourceObjects;
            transforms.SetTransform(index: 0, aim);
            constraint.data.sourceObjects = transforms;
        }
        rigBuilder.Build();
    }
}
