using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class EnemyPool : MonoBehaviour
{
    [SerializeField] private List<MonoBehaviour> prefabs = new List<MonoBehaviour>();
    [SerializeField] private int initialSizePerPrefab = 12;

    private readonly List<Queue<IPoolableEnemy>> pools = new List<Queue<IPoolableEnemy>>();

    private void Awake()
    {
        pools.Clear();

        for (int i = 0; i < prefabs.Count; i++)
        {
            pools.Add(new Queue<IPoolableEnemy>());
            MonoBehaviour prefabMb = prefabs[i];
            if (prefabMb == null)
                continue;
            if (!(prefabMb is IPoolableEnemy))
                continue;
            for (int initialPrefab = 0; initialPrefab < initialSizePerPrefab; initialPrefab++)
            {
                IPoolableEnemy enemy = CreateInstance(i);
                if (enemy != null)
                    pools[i].Enqueue(enemy);
            }
        }
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
    public int PrefabCount => prefabs.Count;
}