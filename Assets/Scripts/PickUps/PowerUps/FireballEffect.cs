using UnityEngine;

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
            Debug.LogWarning("FireballEffect: no fire bullet assigned on player");
            return;
        }
        shooter?.ApplyOverride(FireballShotsPerSecond, 1, 0f, fireballType);
        if (BulletManager.Instance != null)
            BulletManager.Instance.UseFireballMode = true;
        else
            Debug.LogWarning("no BulletManager instance");
    }

    public override void Remove()
    {
        shooter?.ClearOverride();
        if (BulletManager.Instance != null)
            BulletManager.Instance.UseFireballMode = false;
    }
}