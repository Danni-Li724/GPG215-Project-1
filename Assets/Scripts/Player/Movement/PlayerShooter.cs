using UnityEngine;

public sealed class PlayerShooter : MonoBehaviour, ITickable
{
    [Header("Refs")]
    [SerializeField] private BulletManager bulletManager;
    [SerializeField] private Transform muzzle;

    [Header("Type")]
    [SerializeField] private BulletTypeSO bulletType;

    [Header("Fire Params")]
    [SerializeField] private bool alwaysShooting = true;
    [SerializeField] private float shotsPerSecond = 10f;

    [Header("Burst")]
    [SerializeField] private int bulletsPerShot = 5;
    [SerializeField] private float spreadDegrees = 40f;

    private float fireTimer;

    public void Tick(float deltaTime)
    {
        if (!alwaysShooting)
            return;

        if (shotsPerSecond <= 0f)
            return;

        fireTimer += deltaTime;

        float interval = 1f / shotsPerSecond;

        while (fireTimer >= interval)
        {
            fireTimer -= interval;
            FireOnce();
        }
    }

    private void FireOnce()
    {
        if (bulletManager == null || muzzle == null || bulletType == null)
            return;
        Vector2 spawnPos = muzzle.position;
        Vector2 direction = GetShootDirection();
        bulletManager.Spawn(bulletType, spawnPos, direction);
    }

    private Vector2 GetShootDirection()
    {
        return Vector2.up;
    }
}

