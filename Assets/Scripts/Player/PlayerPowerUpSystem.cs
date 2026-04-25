using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPowerUpSystem : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerShooter shooter;
    [SerializeField] private ShieldHealth  shield;
    [SerializeField] private BulletTypeSO  fireballBulletType;
    [SerializeField] private float triShotSpread       = 40f;
    [SerializeField] private float rapidFireMultiplier = 2.5f;
    [Header("Fallback Durations (used if DB unavailable)")]
    [SerializeField] private float triShotDuration    = 8f;
    [SerializeField] private float rapidFireDuration  = 6f;
    [SerializeField] private float shieldDuration     = 12f;
    [SerializeField] private float fireballDuration   = 10f;

    private readonly List<PowerUpEffect> activeEffects = new List<PowerUpEffect>();

    public void Activate(PowerUpType type)
    {
        float duration   = GetDurationFromDB(type);
        PowerUpEffect effect = BuildEffect(type);
        if (effect == null) return;

        effect.Init(shooter, duration);
        effect.Apply();

        activeEffects.Add(effect);
        StartCoroutine(ExpireAfter(effect, duration));
    }

    private IEnumerator ExpireAfter(PowerUpEffect effect, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        effect.Remove();
        activeEffects.Remove(effect);

        bool anyShooterEffect = activeEffects.Exists(e =>
            e is TriShotEffect || e is RapidFireEffect || e is FireballEffect);
        if (!anyShooterEffect)
            shooter?.ClearOverride();
    }

    private PowerUpEffect BuildEffect(PowerUpType type)
    {
        return type switch
        {
            PowerUpType.TriShot   => new TriShotEffect(triShotSpread),
            PowerUpType.RapidFire => new RapidFireEffect(rapidFireMultiplier),
            PowerUpType.Shield    => new ShieldEffect(shield),
            PowerUpType.Fireball  => new FireballEffect(fireballBulletType),
            _                     => null
        };
    }
    
    private float GetDurationFromDB(PowerUpType type)
    {
        if (GameDatabase.Instance != null && GameDatabase.Instance.IsReady)
        {
            ItemRow row = GameDatabase.Instance.GetItem(type.ToString().ToLower());
            if (row != null) return row.duration;
        }
        return type switch
        {
            PowerUpType.TriShot   => triShotDuration,
            PowerUpType.RapidFire => rapidFireDuration,
            PowerUpType.Shield    => shieldDuration,
            PowerUpType.Fireball  => fireballDuration,
            _                     => 5f
        };
    }
}