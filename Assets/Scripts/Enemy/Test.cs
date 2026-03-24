using UnityEngine;

public class Test : MonoBehaviour
{
    
    [SerializeField] private EnemyManager enemyManager;

    public void Spawn()
    {
        if (enemyManager != null)
            // enemyManager.SpawnRandomEnemy();
            enemyManager.SpawnInOrder();
    }
    
    public void SpawnTen()
    {
        if (enemyManager == null)
            return;

        for (int i = 0; i < 10; i++)
        {
            enemyManager.SpawnInOrder();
        }
    }
    public void LogSpawnedCount()
    {
        if (enemyManager == null) return;
        Debug.Log("current active enemy count: " + enemyManager.CurrentActiveCount);
    }
}
