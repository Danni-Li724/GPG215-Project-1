using System.Collections.Generic;
using UnityEngine;
public class EnemySpawnSystem : MonoBehaviour, ITickable
{
    [System.Serializable]
    public class EnemySpawnConfig
    {
        public string label;           
        public int poolIndex;          
        // spawns per minute at time t following the simple linear equation y = at + b
        public float rateMultiplier;     
        public float baseSpawnsPerMinute; 
        public float maxSpawnsPerMinute;  
    }

    [Header("Spawn Configs")]
    [SerializeField] private List<EnemySpawnConfig> configs = new List<EnemySpawnConfig>();

    [Header("Refs")]
    [SerializeField] private EnemyManager enemyManager;
    [SerializeField] private PerformanceTracker performanceTracker;

    [Header("Spawn Area")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnRadius = 3f;
    private float[] spawnAccumulators;
    private float elapsedTime;
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
        float performanceBonus = perfScore * maxPerformanceBonus;

        for (int i = 0; i < configs.Count; i++)
        {
            EnemySpawnConfig cfg = configs[i];
            float tMinutes = t / 60f;
            float spawnsPerMinute = cfg.baseSpawnsPerMinute + cfg.rateMultiplier * tMinutes + performanceBonus;
            spawnsPerMinute = Mathf.Clamp(spawnsPerMinute, 0f, cfg.maxSpawnsPerMinute);
            float spawnsPerSecond = spawnsPerMinute / 60f;
            spawnAccumulators[i] += spawnsPerSecond * dt;
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
    public void ResetSpawner()
    {
        elapsedTime = 0f;
        if (spawnAccumulators != null)
            for (int i = 0; i < spawnAccumulators.Length; i++)
                spawnAccumulators[i] = 0f;
    }
}
