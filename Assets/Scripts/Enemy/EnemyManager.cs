using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour, ITickable
{
    [SerializeField] private EnemyPool pool;
    [SerializeField] private Transform player;

    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnPos;
    [SerializeField] private float spawnRadius = 6f;

    private readonly List<ITickable> active = new List<ITickable>(24);
    private int nextSpawnIndex;
    public int EnemiesKilled { get; private set; }
    public int CurrentActiveCount => active.Count;

    public void Tick(float dt)
    {
        for (int i = active.Count - 1; i >= 0; i--)
        {
            ITickable enemyTickable = active[i];
            if (enemyTickable == null) { active.RemoveAt(i); continue; }

            MonoBehaviour mb = enemyTickable as MonoBehaviour;
            if (mb == null || !mb.gameObject.activeInHierarchy)
            {
                active.RemoveAt(i);
                EnemiesKilled++;
                continue;
            }
            enemyTickable.Tick(dt);
        }
    }
    
    public void SpawnAtPosition(int poolIndex, Vector2 pos)
    {
        if (pool == null || player == null) return;

        IPoolableEnemy pooled = pool.Get(poolIndex);
        if (pooled == null) return;

        IEnemyActivatable activatable = pooled as IEnemyActivatable;
        ITickable tickable            = pooled as ITickable;
        MonoBehaviour mb              = pooled as MonoBehaviour;

        if (activatable == null || tickable == null || mb == null)
        {
            pool.Return(pooled);
            return;
        }

        activatable.Activate(pos, player, pool);
        active.Add(tickable);
    }

    public void SpawnInOrder()
    {
        if (pool == null || player == null || spawnPos == null) return;
        int count = pool.PrefabCount;
        if (count <= 0) return;
        Vector2 pos = new Vector2(
            Random.Range(spawnPos.position.x - spawnRadius, spawnPos.position.x + spawnRadius),
            spawnPos.position.y);
        SpawnAtPosition(nextSpawnIndex % count, pos);
        nextSpawnIndex = (nextSpawnIndex + 1) % count;
    }
}