using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    [SerializeField] private TestEnemy prefab;
    [SerializeField] private int initialSize = 24;

    private readonly Queue<TestEnemy> pool = new Queue<TestEnemy>();

    private void Awake()
    {
        for (int i = 0; i < initialSize; i++)
        {
            TestEnemy enemy = Instantiate(prefab, transform);
            enemy.gameObject.SetActive(false);
            pool.Enqueue(enemy);
        }
    }

    public TestEnemy Get()
    {
        if (pool.Count > 0)
            return pool.Dequeue();

        TestEnemy enemy = Instantiate(prefab, transform);
        enemy.gameObject.SetActive(false);
        return enemy;
    }

    public void Return(TestEnemy enemy)
    {
        enemy.gameObject.SetActive(false);
        pool.Enqueue(enemy);
    }
}