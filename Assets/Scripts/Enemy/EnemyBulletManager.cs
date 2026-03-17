using System.Collections.Generic;
using UnityEngine;

public class EnemyBulletManager : MonoBehaviour, ITickable
{
    [SerializeField] private EnemyBulletPool pool;
    [SerializeField] private float bulletSpeed = 8f;
    [SerializeField] private float bulletLifetime = 3f;

    private readonly List<EnemyBullet> active = new List<EnemyBullet>(256);

    public void Spawn(Vector2 position, Vector2 direction)
    {
        if (pool == null)
            return;

        EnemyBullet bullet = pool.Get();
        bullet.Activate(position, direction, bulletSpeed, bulletLifetime);
        active.Add(bullet);
    }

    public void Tick(float dt)
    {
        for (int i = active.Count - 1; i >= 0; i--)
        {
            EnemyBullet bullet = active[i];
            if (bullet == null || !bullet.IsActive)
            {
                active.RemoveAt(i);
                continue;
            }

            bullet.Tick(dt);

            if (!bullet.IsActive)
            {
                active.RemoveAt(i);
                pool.Return(bullet);
            }
        }
    }
}
