using System.Data;
using UnityEngine;
using Zenject;

public class PlayerSpawner
{
    [Inject] private readonly Player.Factory _playerFactory;

    [Inject] private readonly Transform _spawnPosition;

    public PlayerSpawner() => Player = null;

    public Player Player { get; private set; }

    public bool PlayerActive { get => Player?.gameObject.activeSelf ?? false; }

    public void Spawn()
    {
        if (Player != null) throw new DataException("Player instance already exists");
        Player = _playerFactory.Create();
        Player.Place(_spawnPosition);
    }

    public void Respawn()
    {
        Player.Place(_spawnPosition);
        Player.ResetStats();
        Player.Resurrect();
    }

    public void DeactivatePlayer() => Player.Die();
}
