using UnityEngine;

public class Test : MonoBehaviour
{
    
    [SerializeField] private EnemyManager enemyManager;

    public void Spawn()
    {
        if (enemyManager != null)
            enemyManager.SpawnTestEnemy();
    }
}
