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
    private float fireTimer;

    private float baseShotsPerSecond;
    private BulletTypeSO baseBulletType;
    private float activeShotsPerSecond;
    private int   activeBulletsPerShot;
    private float activeSpreadDegrees;
    private BulletTypeSO activeBulletType;

    public float BaseShotsPerSecond => baseShotsPerSecond;

    private void Awake()
    {
        baseShotsPerSecond = shotsPerSecond;
        baseBulletType     = bulletType;
        ClearOverride(); 
    }

    public void ApplyOverride(float newShotsPerSecond, int newBulletsPerShot,
                              float newSpreadDegrees, BulletTypeSO newBulletType)
    {
        if (newShotsPerSecond > 0f)  activeShotsPerSecond = newShotsPerSecond;
        if (newBulletsPerShot > 0)   activeBulletsPerShot = newBulletsPerShot;
        if (newSpreadDegrees >= 0f)  activeSpreadDegrees  = newSpreadDegrees;
        if (newBulletType != null)   activeBulletType     = newBulletType;
    }

    public void ClearOverride()
    {
        activeShotsPerSecond = baseShotsPerSecond;
        activeBulletsPerShot = 1;      
        activeSpreadDegrees  = 0f;
        activeBulletType     = baseBulletType;
    }

    public void Tick(float deltaTime)
    {
        if (!alwaysShooting || activeShotsPerSecond <= 0f) return;
        fireTimer += deltaTime;
        float interval = 1f / activeShotsPerSecond;
        while (fireTimer >= interval)
        {
            fireTimer -= interval;
            FireOnce();
        }
    }

    private void FireOnce()
    {
        if (bulletManager == null || muzzle == null || activeBulletType == null) return;
        Vector2 spawnPos = muzzle.position;

        if (activeBulletsPerShot <= 1)
        {
            bulletManager.SpawnBullet(activeBulletType, spawnPos, Vector2.up);
            return;
        }
        float halfSpread = activeSpreadDegrees * 0.5f;
        float step = activeBulletsPerShot > 1
            ? activeSpreadDegrees / (activeBulletsPerShot - 1) : 0f;
        for (int i = 0; i < activeBulletsPerShot; i++)
        {
            float angle = -halfSpread + step * i;
            Vector2 dir = Quaternion.Euler(0f, 0f, angle) * Vector2.up;
            bulletManager.SpawnBullet(activeBulletType, spawnPos, dir);
        }
    }
}