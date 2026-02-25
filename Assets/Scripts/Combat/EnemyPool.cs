using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class EnemyPool : MonoBehaviour
{
    [SerializeField] private List<TestEnemy> prefabs = new List<TestEnemy>();
    [SerializeField] private int initialSizePerPrefab = 12;

    private readonly List<Queue<TestEnemy>> pools = new List<Queue<TestEnemy>>();

    private void Awake()
    {
        pools.Clear();

        for (int i = 0; i < prefabs.Count; i++)
        {
            pools.Add(new Queue<TestEnemy>());
            for (int initPrefab = 0; initPrefab < initialSizePerPrefab; initPrefab++)
            {
                TestEnemy enemy = Instantiate(prefabs[i], transform);
                enemy.gameObject.SetActive(false);
                enemy.SetPoolIndex(i);
                pools[i].Enqueue(enemy);
            }
        }
    }

    public TestEnemy Get(int prefabIndex)
    {
        if (prefabIndex < 0 || prefabIndex >= prefabs.Count)
            return null;

        if (pools[prefabIndex].Count > 0)
            return pools[prefabIndex].Dequeue();

        TestEnemy enemy = Instantiate(prefabs[prefabIndex], transform);
        enemy.gameObject.SetActive(false);
        enemy.SetPoolIndex(prefabIndex);
        return enemy;
    }

    public void Return(TestEnemy enemy)
    {
        if (enemy == null)
            return;

        int index = enemy.PoolIndex;
        if (index < 0 || index >= pools.Count)
        {
            enemy.gameObject.SetActive(false);
            return;
        }

        enemy.gameObject.SetActive(false);
        pools[index].Enqueue(enemy);
    }
    public int PrefabCount => prefabs.Count;
}