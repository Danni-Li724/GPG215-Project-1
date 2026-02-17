using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour, ITickable
{
    [SerializeField] private EnemyPool pool;
    [SerializeField] private Transform player;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnEverySeconds = 1.0f;
    [SerializeField] private float spawnRadius = 6f;
    [SerializeField] private Transform spawnPos;
    private float spawnTimer;
    private readonly List<TestEnemy> active = new List<TestEnemy>(64);

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
            TestEnemy enemy = active[i];

            if (enemy == null || !enemy.IsActive)
            {
                active.RemoveAt(i);
                continue;
            }

            enemy.Tick(dt);
            if (!enemy.IsActive)
                active.RemoveAt(i);
        }
    }
    public void SpawnTestEnemy()
    {
        if (pool == null || player == null)
            return;

        Vector2 center = player.position;
        Vector2 offset = Random.insideUnitCircle.normalized * spawnRadius;
        // Vector2 pos = center + offset;
        Vector2 pos = new Vector2(Random.Range(spawnPos.position.x - spawnRadius, spawnPos.position.x + spawnRadius), spawnPos.position.y);
        TestEnemy enemy = pool.Get();
        enemy.Activate(pos, player, pool);
        active.Add(enemy);
    }
}
