using UnityEngine;

// Switches PlayerShooter to slow single shots using the fireball BulletTypeSO.
// Sets BulletManager.UseFireballMode so spawned bullets get tagged for
// FireStatusEffect on hit.
public class FireballEffect : PowerUpEffect
{
    private readonly BulletTypeSO fireballType;
    private const float FireballShotsPerSecond = 1.2f;

    public FireballEffect(BulletTypeSO fireballType)
    {
        this.fireballType = fireballType;
    }

    public override void Apply()
    {
        if (fireballType == null)
        {
            Debug.LogWarning("FireballEffect: no fireballBulletType assigned on PlayerPowerUpSystem");
            return;
        }
        shooter?.ApplyOverride(FireballShotsPerSecond, 1, 0f, fireballType);
        if (BulletManager.Instance != null)
            BulletManager.Instance.UseFireballMode = true;
        else
            Debug.LogWarning("FireballEffect: BulletManager.Instance is null — ensure BulletManager is in scene");
    }

    public override void Remove()
    {
        shooter?.ClearOverride();
        if (BulletManager.Instance != null)
            BulletManager.Instance.UseFireballMode = false;
    }
}