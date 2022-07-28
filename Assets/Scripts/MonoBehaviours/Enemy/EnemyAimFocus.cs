using System.Collections;
using UnityEngine;
using Zenject;

public class EnemyAimFocus : CharacterAim
{
    [Inject] private readonly PlayerSpawner _playerSpawner;

    private void Start() => StartCoroutine(setPlayerAsAim());

    private IEnumerator setPlayerAsAim()
    {
        yield return new WaitUntil(() => _playerSpawner.Player != null);
        SetAim(_playerSpawner.Player.Head);
    }
}
