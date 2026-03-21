using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour, ITickable
{
    [SerializeField] private EnemyPool pool;
    [SerializeField] private Transform player;

    [Header("SpawnBullet Settings")]
    [SerializeField] private float spawnEverySeconds = 1.0f;
    [SerializeField] private float spawnRadius = 6f;
    [SerializeField] private Transform spawnPos;
    private float spawnTimer;
    private readonly List<ITickable> active = new List<ITickable>(24);
    
    private int nextSpawnIndex;
    public int EnemiesKilled { get; private set; }

    public void Tick(float dt)
    {
        spawnTimer += dt;
        while (spawnTimer >= spawnEverySeconds)
        {
            spawnTimer -= spawnEverySeconds;
            // SpawnTestEnemy();
        }
        for (int i = active.Count - 1; i >= 0; i--)
        {
            ITickable enemyTickable = active[i];

            if (enemyTickable == null)
            {
                active.RemoveAt(i);
                continue;
            }
            IPoolableEnemy poolable = enemyTickable as IPoolableEnemy;
            MonoBehaviour enemyMonobehaviour = enemyTickable as MonoBehaviour;
            if (enemyMonobehaviour == null || !enemyMonobehaviour.gameObject.activeInHierarchy)
            {
                active.RemoveAt(i);
                EnemiesKilled += 1;
                continue;
            }
            enemyTickable.Tick(dt);
        }
    }
    // public void SpawnTestEnemy()
    // {
    //     if (pool == null || player == null)
    //         return;
    //
    //     Vector2 center = player.position;
    //     Vector2 offset = Random.insideUnitCircle.normalized * spawnRadius;
    //     // Vector2 pos = center + offset;
    //     Vector2 pos = new Vector2(Random.Range(spawnPos.position.x - spawnRadius, spawnPos.position.x + spawnRadius), spawnPos.position.y);
    //     BaseClasher enemy = pool.Get();
    //     enemy.Activate(pos, player, pool);
    //     active.Add(enemy);
    // }
    
    // public void SpawnRandomEnemy()
    // {
    //     if (pool == null || player == null || spawnPos == null)
    //         return;
    //
    //     if (pool.PrefabCount <= 0)
    //         return;
    //
    //     int randomIndex = Random.Range(0, pool.PrefabCount);
    //
    //     Vector2 pos = new Vector2(
    //         Random.Range(spawnPos.position.x - spawnRadius, spawnPos.position.x + spawnRadius),
    //         spawnPos.position.y
    //     );
    //
    //     IPoolableEnemy pooled = pool.Get(randomIndex);
    //     if (pooled == null)
    //         return;
    //     MonoBehaviour enemyMonobehaviour = pooled as MonoBehaviour;
    //     if (enemyMonobehaviour == null)
    //         return;
    //     BaseClasher baseClasher = pooled as BaseClasher;
    //     if (baseClasher != null)
    //     {
    //         baseClasher.Activate(pos, player, pool);
    //         active.Add(baseClasher);
    //         return;
    //     }
    //     DefaultRangerContext ranger = pooled as DefaultRangerContext;
    //     if (ranger != null)
    //     {
    //         ranger.Activate(pos, player, pool);
    //         active.Add(ranger);
    //         return;
    //     }
    //     enemyMonobehaviour.gameObject.SetActive(false);
    //     pool.Return(pooled);
    // }
    
    public void SpawnRandomEnemy()
    {
        if (pool == null || player == null || spawnPos == null)
            return;

        int count = pool.PrefabCount;
        if (count <= 0)
            return;

        Vector2 pos = new Vector2(
            Random.Range(spawnPos.position.x - spawnRadius, spawnPos.position.x + spawnRadius),
            spawnPos.position.y
        );

        // trying multiple indices so it doesn't "fail spawn" for new types
        int startIndex = Random.Range(0, count);

        for (int attempt = 0; attempt < count; attempt++)
        {
            int index = (startIndex + attempt) % count;

            IPoolableEnemy pooled = pool.Get(index);
            if (pooled == null)
                continue;

            IEnemyActivatable activatable = pooled as IEnemyActivatable;
            ITickable tickable = pooled as ITickable;
            MonoBehaviour mb = pooled as MonoBehaviour;

            if (activatable == null || tickable == null || mb == null)
            {
                pool.Return(pooled);
                continue;
            }

            activatable.Activate(pos, player, pool);
            active.Add(tickable);
            return;
        }
    }

    public void SpawnInOrder()
    {
        if (pool == null || player == null || spawnPos == null)
            return;

        int count = pool.PrefabCount;
        if (count <= 0)
            return;

        Vector2 pos = new Vector2(
            Random.Range(spawnPos.position.x - spawnRadius, spawnPos.position.x + spawnRadius),
            spawnPos.position.y
        );
        
        for (int attempt = 0; attempt < count; attempt++)
        {
            int index = (nextSpawnIndex + attempt) % count;

            IPoolableEnemy pooled = pool.Get(index);
            if (pooled == null)
                continue;
            IEnemyActivatable activatable = pooled as IEnemyActivatable;
            ITickable tickable = pooled as ITickable;
            MonoBehaviour mb = pooled as MonoBehaviour;

            if (activatable == null || tickable == null || mb == null)
            {
                pool.Return(pooled);
                continue;
            }

            activatable.Activate(pos, player, pool);
            active.Add(tickable);
            nextSpawnIndex = (index + 1) % count; // advance to the next index
            return;
        }
    }
}
