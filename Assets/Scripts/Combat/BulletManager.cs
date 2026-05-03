using System.Collections.Generic;
using UnityEngine;

public sealed class BulletManager : MonoBehaviour, ITickable
{
    [SerializeField] private BulletPool pool;
    [SerializeField] private PlayerPowerUpSystem powerUpSystem;
    [SerializeField] private GameObject fireEffectPrefab;

    public static BulletManager Instance { get; private set; }
    public bool UseFireballMode { get; set; }

    private readonly List<Bullet> active = new List<Bullet>(256);

    private void Awake() { Instance = this; }

    public void SpawnBullet(BulletTypeSO type, Vector2 position, Vector2 direction)
    {
        if (pool == null || type == null) return;
        Bullet bullet = pool.Get();
        bullet.Activate(type, position, direction);
        if (UseFireballMode && fireEffectPrefab != null)
        {
            bullet.IsFireball      = true;
            bullet.FireEffectPrefab = fireEffectPrefab;
        }

        active.Add(bullet);
    }

    public void Tick(float deltaTime)
    {
        for (int i = active.Count - 1; i >= 0; i--)
        {
            Bullet bullet = active[i];
            if (bullet == null || !bullet.IsActive) { active.RemoveAt(i); continue; }
            bullet.Tick(deltaTime);
            if (!bullet.IsActive) { active.RemoveAt(i); pool.Return(bullet); }
        }
    }
}