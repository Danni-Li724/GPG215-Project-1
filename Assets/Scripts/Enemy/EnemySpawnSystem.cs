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
    // [SerializeField] private List<EnemySpawnConfig> configs = new List<EnemySpawnConfig>();
    private List<EnemySpawnConfig> activeConfigs = new List<EnemySpawnConfig>();
    
    [Header("Rate Template")]
    [SerializeField] private List<EnemySpawnConfig> rateTemplates = new List<EnemySpawnConfig>(); // one per enemy in LevelEnemiesSO.AllEnemies() order

    [Header("Refs")]
    [SerializeField] private EnemyManager enemyManager;
    [SerializeField] private PerformanceTracker performanceTracker;

    [Header("Spawn Area")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnRadius = 3f;
    private float[] spawnAccumulators;
    private float elapsedTime;
    [SerializeField] private float maxPerformanceBonus = 10f;

    // private void Awake()
    // {
    //     spawnAccumulators = new float[configs.Count];
    // }
    
    public void SetupForLevel(LevelEnemiesSO levelEnemies)
    {
        activeConfigs.Clear();

        List<GameObject> all = levelEnemies != null ? levelEnemies.AllEnemies() : new List<GameObject>();

        for (int i = 0; i < all.Count; i++)
        {
            // reuse template rates if available, otherwise use defaults
            EnemySpawnConfig template = i < rateTemplates.Count ? rateTemplates[i] : null;

            activeConfigs.Add(new EnemySpawnConfig
            {
                label               = all[i] != null ? all[i].name : $"Enemy_{i}",
                poolIndex           = i,                                                        
                baseSpawnsPerMinute = template != null ? template.baseSpawnsPerMinute : 3f,
                rateMultiplier      = template != null ? template.rateMultiplier      : 0.5f,
                maxSpawnsPerMinute  = template != null ? template.maxSpawnsPerMinute  : 10f,
            });
        }

        spawnAccumulators = new float[activeConfigs.Count];
        Debug.Log($"EnemySpawnSystem: {activeConfigs.Count} configs for level");
    }

    public void Tick(float dt)
    {
        elapsedTime += dt;
        float t = elapsedTime;
        float perfScore = performanceTracker != null ? performanceTracker.PerformanceScore : 0.5f;
        float performanceBonus = perfScore * maxPerformanceBonus;

        for (int i = 0; i < activeConfigs.Count; i++)
        {
            EnemySpawnConfig cfg = activeConfigs[i];
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
