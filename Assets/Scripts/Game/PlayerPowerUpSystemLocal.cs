using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Replaces PlayerPowerUpSystem. Reads powerup durations from ItemSO assets
// instead of the SQLite database.
public class PlayerPowerUpSystemLocal : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerShooter shooter;
    [SerializeField] private ShieldHealth  shield;
    [SerializeField] private BulletTypeSO  fireballBulletType;

    [Header("Item SOs — assign one per powerup type")]
    [SerializeField] private ItemSO triShotItem;
    [SerializeField] private ItemSO rapidFireItem;
    [SerializeField] private ItemSO shieldItem;
    [SerializeField] private ItemSO fireballItem;

    [Header("Powerup Settings")]
    [SerializeField] private float triShotSpread       = 40f;
    [SerializeField] private float rapidFireMultiplier = 2.5f;

    [Header("Fallback Durations (used if ItemSO not assigned)")]
    [SerializeField] private float triShotDuration   = 8f;
    [SerializeField] private float rapidFireDuration  = 6f;
    [SerializeField] private float shieldDuration     = 12f;
    [SerializeField] private float fireballDuration   = 10f;

    private readonly List<PowerUpEffect> activeEffects = new List<PowerUpEffect>();

    public void Activate(PowerUpType type)
    {
        float         duration = GetDuration(type);
        PowerUpEffect effect   = BuildEffect(type);
        if (effect == null) return;

        effect.Init(shooter, duration);
        effect.Apply();
        activeEffects.Add(effect);
        StartCoroutine(ExpireAfter(effect, duration));
    }

    public void ApplyFireStatusToEnemy(GameObject enemyGO)
    {
        if (fireballItem == null) return;
        if (enemyGO.GetComponentInChildren<FireEffect>() != null) return;

        FireEffect fx = Instantiate(
            Resources.Load<GameObject>("FireEffectPrefab"),
            null).GetComponent<FireEffect>();
        if (fx != null) fx.Activate(enemyGO.transform);
    }

    private IEnumerator ExpireAfter(PowerUpEffect effect, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        effect.Remove();
        activeEffects.Remove(effect);

        bool anyShooterEffect = activeEffects.Exists(e =>
            e is TriShotEffect || e is RapidFireEffect || e is FireballEffect);
        if (!anyShooterEffect) shooter?.ClearOverride();
    }

    private PowerUpEffect BuildEffect(PowerUpType type) => type switch
    {
        PowerUpType.TriShot   => new TriShotEffect(triShotSpread),
        PowerUpType.RapidFire => new RapidFireEffect(rapidFireMultiplier),
        PowerUpType.Shield    => new ShieldEffect(shield),
        PowerUpType.Fireball  => new FireballEffect(fireballBulletType),
        _                     => null
    };

    private float GetDuration(PowerUpType type) => type switch
    {
        PowerUpType.TriShot   => triShotItem   != null ? triShotItem.duration   : triShotDuration,
        PowerUpType.RapidFire => rapidFireItem != null ? rapidFireItem.duration  : rapidFireDuration,
        PowerUpType.Shield    => shieldItem    != null ? shieldItem.duration     : shieldDuration,
        PowerUpType.Fireball  => fireballItem  != null ? fireballItem.duration   : fireballDuration,
        _                     => 5f
    };
}
