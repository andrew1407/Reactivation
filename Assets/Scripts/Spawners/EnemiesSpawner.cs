using System.Data;
using UnityEngine;
using Zenject;

public class EnemiesSpawner
{
    [Inject] private readonly Enemy.Factory _enemyFactory;

    [Inject] private readonly Transform _spawnPosition;

    [Inject] private readonly uint _enemiesCount;

    private ObjectPool<Enemy> _pool;

    public bool AllDefeated { get => _pool.Reserved == 0; }

    public void GeneratePool(Player player)
    {
        if (_pool != null) throw new DataException("Enemies have been already spawned");
        Transform target = player.transform;
        Transform lookAt = player.Head;
        _pool = new(_enemiesCount, () =>
        {
            Enemy enemy = _enemyFactory.Create();
            enemy.Place(_spawnPosition);
            enemy.gameObject.SetActive(false);
            return enemy;
        });
    }

    public void Spawn()
    {
        foreach (var enemy in _pool.GetReserved()) DeactivateEnemy(enemy, killed: false);
        while (_pool.Free > 0)
        {
            Enemy enemy = _pool.Give();
            enemy.Place(_spawnPosition);
            enemy.Resurrect();
        }
    }

    public void DeactivateEnemy(Enemy enemy, bool killed = true)
    {
        bool put = _pool.Put(enemy);
        if (!put) return;
        enemy.Die(killed);
    }
}
