using System.Collections.Generic;
using UnityEngine;

// Replaces the manual Test.cs spawn buttons.
// Implements ITickable so GameManager drives it via the tickable list.
// Spawn rate per enemy type follows: y = a*t + b
//   a = rateMultiplier (designer-set per type, controls how fast spawning scales over time)
//   t = elapsed game time in seconds
//   b = performanceBonus (0 if player is struggling, scales up if player is doing well)
// This means early game is slow and controlled, late game ramps up naturally.
// performanceBonus is fed by PerformanceTracker each frame.
public class EnemySpawnSystem : MonoBehaviour, ITickable
{
    [System.Serializable]
    public class EnemySpawnConfig
    {
        public string label;              // just for inspector readability
        public int poolIndex;             // matches EnemyPool prefab index
        // y = a*t + b  — spawns per minute at time t
        public float rateMultiplier;      // a: how fast spawn rate grows over time
        public float baseSpawnsPerMinute; // base rate at t=0 (can be 0 for rare enemies)
        public float maxSpawnsPerMinute;  // hard cap so late-game doesn't get insane
    }

    [Header("Spawn Configs — one entry per enemy type in EnemyPool order")]
    [SerializeField] private List<EnemySpawnConfig> configs = new List<EnemySpawnConfig>();

    [Header("Refs")]
    [SerializeField] private EnemyManager enemyManager;
    [SerializeField] private PerformanceTracker performanceTracker;

    [Header("Spawn Area")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnRadius = 3f;

    // per-type spawn timers — tracks fractional spawns between ticks
    private float[] spawnAccumulators;
    private float elapsedTime;

    // performance bonus range — at score 1.0 adds this many extra spawns/min
    [SerializeField] private float maxPerformanceBonus = 10f;

    private void Awake()
    {
        spawnAccumulators = new float[configs.Count];
    }

    public void Tick(float dt)
    {
        elapsedTime += dt;
        float t = elapsedTime;
        float perfScore = performanceTracker != null ? performanceTracker.PerformanceScore : 0.5f;
        // b scales with performance — struggling player gets b=0, acing it gets full bonus
        float performanceBonus = perfScore * maxPerformanceBonus;

        for (int i = 0; i < configs.Count; i++)
        {
            EnemySpawnConfig cfg = configs[i];

            // y = a*t + b  (in spawns per minute)
            float tMinutes = t / 60f;
            float spawnsPerMinute = cfg.baseSpawnsPerMinute + cfg.rateMultiplier * tMinutes + performanceBonus;
            spawnsPerMinute = Mathf.Clamp(spawnsPerMinute, 0f, cfg.maxSpawnsPerMinute);

            float spawnsPerSecond = spawnsPerMinute / 60f;
            spawnAccumulators[i] += spawnsPerSecond * dt;

            // spawn whole units, carry remainder forward
            while (spawnAccumulators[i] >= 1f)
            {
                spawnAccumulators[i] -= 1f;
                SpawnEnemy(cfg.poolIndex);
            }
        }
    }

    private void SpawnEnemy(int poolIndex)
    {
        if (enemyManager == null || spawnPoint == null) return;
        Vector2 pos = new Vector2(
            Random.Range(spawnPoint.position.x - spawnRadius, spawnPoint.position.x + spawnRadius),
            spawnPoint.position.y
        );
        enemyManager.SpawnAtPosition(poolIndex, pos);
    }

    // Called by GameManager.BeginRun() to reset timers when a new run starts
    public void ResetSpawner()
    {
        elapsedTime = 0f;
        if (spawnAccumulators != null)
            for (int i = 0; i < spawnAccumulators.Length; i++)
                spawnAccumulators[i] = 0f;
    }
}
