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
    //     TestEnemy enemy = pool.Get();
    //     enemy.Activate(pos, player, pool);
    //     active.Add(enemy);
    // }
    
    public void SpawnRandomEnemy()
    {
        if (pool == null || player == null || spawnPos == null)
            return;

        if (pool.PrefabCount <= 0)
            return;

        int randomIndex = Random.Range(0, pool.PrefabCount);

        Vector2 pos = new Vector2(
            Random.Range(spawnPos.position.x - spawnRadius, spawnPos.position.x + spawnRadius),
            spawnPos.position.y
        );

        IPoolableEnemy pooled = pool.Get(randomIndex);
        if (pooled == null)
            return;
        MonoBehaviour enemyMonobehaviour = pooled as MonoBehaviour;
        if (enemyMonobehaviour == null)
            return;
        TestEnemy testEnemy = pooled as TestEnemy;
        if (testEnemy != null)
        {
            testEnemy.Activate(pos, player, pool);
            active.Add(testEnemy);
            return;
        }
        DefaultRangerContext ranger = pooled as DefaultRangerContext;
        if (ranger != null)
        {
            ranger.Activate(pos, player, pool);
            active.Add(ranger);
            return;
        }
        enemyMonobehaviour.gameObject.SetActive(false);
        pool.Return(pooled);
    }
}
