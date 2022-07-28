using UnityEngine;
using Zenject;

public class EnemyActions : MonoBehaviour
{
    [Inject] private readonly GameManager _gameManager;

    [Inject] private readonly PlayerSpawner _playerSpawner;

    [Inject] private readonly EnemyHealthController _healthController;

    [Inject] private readonly EnemyActionsStrategy _actionsStrategy;

    private Transform _cameraTransform;

    [Inject]
    private void Construct(CameraParams cameraParams)
    {
        _cameraTransform = cameraParams.Camera.transform;
    }

    void Update()
    {
        if (_gameManager.GamePaused) return;
        var player = _playerSpawner.Player;
        bool playerACtive = _playerSpawner.PlayerActive;
        if (playerACtive != _actionsStrategy.ShouldFollowTarget)
            _actionsStrategy.ShouldFollowTarget = playerACtive;
        _actionsStrategy.FollowTarget(player.transform);
        _actionsStrategy.RotateToTarget(player.Head);
        if (_actionsStrategy.ShouldAttack && playerACtive)
            StartCoroutine(_actionsStrategy.Attack());
    }

    private void LateUpdate() => _healthController.RotateHealthTab(_cameraTransform.position);
}
