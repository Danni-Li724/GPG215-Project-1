using System.Collections.Generic;
using UnityEngine;

public sealed class BulletManager : MonoBehaviour, ITickable
{
    [SerializeField] private BulletPool pool;
    private readonly List<Bullet> active = new List<Bullet>(256);

    public void Spawn(BulletTypeSO type, Vector2 position, Vector2 direction)
    {
        Bullet bullet = pool.Get();
        bullet.Activate(type, position, direction);
        active.Add(bullet);
    }

    public void Tick(float deltaTime)
    {
        for (int i = active.Count - 1; i >= 0; i--)
        {
            Bullet bullet = active[i];

            if (bullet == null || !bullet.IsActive)
            {
                active.RemoveAt(i);
                continue;
            }

            bullet.Tick(deltaTime);

            if (!bullet.IsActive)
            {
                active.RemoveAt(i);
                pool.Return(bullet);
            }
        }
    }
}