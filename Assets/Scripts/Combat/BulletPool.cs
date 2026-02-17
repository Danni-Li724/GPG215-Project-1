using System.Collections.Generic;
using UnityEngine;

public sealed class BulletPool : MonoBehaviour
{
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private int initialSize = 64;
    private readonly Queue<Bullet> pool = new Queue<Bullet>();
    private void Awake()
    {
        WarmUp();
    }

    private void WarmUp()
    {
        for (int i = 0; i < initialSize; i++)
        {
            Bullet bullet = CreateNew();
            bullet.Deactivate();
            pool.Enqueue(bullet);
        }
    }

    private Bullet CreateNew()
    {
        Bullet bullet = Instantiate(bulletPrefab, transform);
        return bullet;
    }

    public Bullet Get()
    {
        if (pool.Count > 0)
            return pool.Dequeue();

        Bullet bullet = CreateNew();
        bullet.Deactivate();
        return bullet;
    }

    public void Return(Bullet bullet)
    {
        bullet.Deactivate();
        pool.Enqueue(bullet);
    }
}

