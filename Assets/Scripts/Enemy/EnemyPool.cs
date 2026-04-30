using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class EnemyPool : MonoBehaviour
{
    // [SerializeField] private List<MonoBehaviour> prefabs = new List<MonoBehaviour>();
    private List<MonoBehaviour>            prefabs = new List<MonoBehaviour>();
    
    [SerializeField] private int initialSizePerPrefab = 12;

    private readonly List<Queue<IPoolableEnemy>> pools = new List<Queue<IPoolableEnemy>>();

    // private void Awake()
    // {
    //     pools.Clear();
    //
    //     for (int i = 0; i < prefabs.Count; i++)
    //     {
    //         pools.Add(new Queue<IPoolableEnemy>());
    //         MonoBehaviour prefabMb = prefabs[i];
    //         if (prefabMb == null)
    //             continue;
    //         if (!(prefabMb is IPoolableEnemy))
    //             continue;
    //         for (int initialPrefab = 0; initialPrefab < initialSizePerPrefab; initialPrefab++)
    //         {
    //             IPoolableEnemy enemy = CreateInstance(i);
    //             if (enemy != null)
    //                 pools[i].Enqueue(enemy);
    //         }
    //     }
    // }
    
    public void SetupForLevel(LevelEnemiesSO levelEnemies)
    {
        ClearAllInstances();
        prefabs.Clear();
        pools.Clear();

        if (levelEnemies == null) return;

        List<GameObject> all = levelEnemies.AllEnemies();
        for (int i = 0; i < all.Count; i++)
        {
            GameObject prefabGO = all[i];
            if (prefabGO == null) continue;

            MonoBehaviour mb = prefabGO.GetComponent<MonoBehaviour>();
            if (mb == null || !(mb is IPoolableEnemy))
            {
                Debug.LogWarning($"EnemyPool: '{prefabGO.name}' missing IPoolableEnemy — skipped");
                continue;
            }

            prefabs.Add(mb);
            pools.Add(new Queue<IPoolableEnemy>());

            int poolIndex = prefabs.Count - 1;
            for (int j = 0; j < initialSizePerPrefab; j++)
            {
                IPoolableEnemy enemy = CreateInstance(poolIndex);
                if (enemy != null) pools[poolIndex].Enqueue(enemy);
            }
        }

        Debug.Log($"EnemyPool: setup — {prefabs.Count} types, {initialSizePerPrefab} each");
    }

    private IPoolableEnemy CreateInstance(int prefabIndex)
    {
        MonoBehaviour prefabMb = prefabs[prefabIndex];
        if (prefabMb == null)
            return null;
        MonoBehaviour instanceMb = Instantiate(prefabMb, transform);
        instanceMb.gameObject.SetActive(false);
        IPoolableEnemy poolable = instanceMb as IPoolableEnemy;
        if (poolable == null)
            return null;
        poolable.SetPoolIndex(prefabIndex);
        return poolable;
    }

    public IPoolableEnemy Get(int prefabIndex)
    {
        if (prefabIndex < 0 || prefabIndex >= prefabs.Count)
            return null;
        if (pools[prefabIndex].Count > 0)
            return pools[prefabIndex].Dequeue();
        return CreateInstance(prefabIndex);
    }

    public void Return(IPoolableEnemy enemy)
    {
        if (enemy == null)
            return;
        int index = enemy.PoolIndex;
        MonoBehaviour enemyMonobehaviour = enemy as MonoBehaviour;
        if (enemyMonobehaviour != null)
            enemyMonobehaviour.gameObject.SetActive(false);
        if (index < 0 || index >= pools.Count)
            return;
        pools[index].Enqueue(enemy);
    }
    
    private void ClearAllInstances()
    {
        foreach (var queue in pools)
        foreach (var enemy in queue)
        {
            MonoBehaviour mb = enemy as MonoBehaviour;
            if (mb != null) Destroy(mb.gameObject);
        }
        pools.Clear();
    }
    public int PrefabCount => prefabs.Count;
}