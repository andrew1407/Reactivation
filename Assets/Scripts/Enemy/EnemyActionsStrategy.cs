using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

public class EnemyActionsStrategy
{
    [Inject(Id = "AttackDistance")] private readonly float _attackDistance;

    [Inject(Id = "AttackFrequency")] private readonly float _attackFrequency;

    [Inject] private readonly CharacterComponents _characterComponents;

    [Inject] private readonly NavMeshAgent _navMeshAgent;

    [Inject] private GameManager _gameManager;

    private readonly int _fistAttackLabelHash = Animator.StringToHash("fist_attack");

    private readonly int _idleHash = Animator.StringToHash("idle");

    private readonly int _xLabeleHash = Animator.StringToHash("InputX");

    private readonly int _yLabeleHash = Animator.StringToHash("InputY");

    private Vector3 _lastPosition;

    public bool ShouldAttack { get => _navMeshAgent.remainingDistance <= _attackDistance; }

    public bool ShouldFollowTarget
    {
        get => !_navMeshAgent.isStopped;
        set => _navMeshAgent.isStopped = !value;
    }

    public bool NavMeshAgentEnabled
    {
        get => _navMeshAgent.enabled;
        set => _navMeshAgent.enabled = value;
    }

    public IEnumerator Attack()
    {
        yield return GameUtils.PlayAnimation(_characterComponents.RigAnimator, _fistAttackLabelHash);
        yield return new WaitForSeconds(_attackFrequency);
        _characterComponents.RigAnimator.Play(_idleHash);
        yield return null;
    }

    public void FollowTarget(Transform target)
    {
        var transform = _characterComponents.Transform;
        bool canMove = _navMeshAgent.enabled && !_navMeshAgent.isStopped;
        Vector2 localDirection = canMove ? setDestination(target) : Vector2.zero;
        _lastPosition = transform.position;
        _characterComponents.BodyAnimator.SetFloat(_xLabeleHash, localDirection.x);
        _characterComponents.BodyAnimator.SetFloat(_yLabeleHash, localDirection.y);
    }

    public void RotateToTarget(Transform target)
    {
        var transform = _characterComponents.Transform;
        float ratio = 2 * Time.fixedDeltaTime;
        var lookPos = target.position - transform.position;
        lookPos.y = 0;
        var rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, ratio);
    }

    private Vector2 setDestination(Transform target)
    {
        var transform = _characterComponents.Transform;
        _navMeshAgent.SetDestination(target.position);
        var direction = transform.position - _lastPosition;
        var localDirection = transform.InverseTransformDirection(direction).normalized;
        return new(localDirection.x, localDirection.z);        
    }
}
