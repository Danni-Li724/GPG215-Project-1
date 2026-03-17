using System.Collections.Generic;
using UnityEngine;

public class EnemyBulletPool : MonoBehaviour
{
    [SerializeField] private EnemyBullet prefab;
    [SerializeField] private int initialSize = 64;

    private readonly Queue<EnemyBullet> pool = new Queue<EnemyBullet>();

    private void Awake()
    {
        for (int i = 0; i < initialSize; i++)
        {
            EnemyBullet b = Instantiate(prefab, transform);
            b.gameObject.SetActive(false);
            pool.Enqueue(b);
        }
    }

    public EnemyBullet Get()
    {
        if (pool.Count > 0)
            return pool.Dequeue();

        EnemyBullet b = Instantiate(prefab, transform);
        b.gameObject.SetActive(false);
        return b;
    }

    public void Return(EnemyBullet b)
    {
        b.gameObject.SetActive(false);
        pool.Enqueue(b);
    }
}