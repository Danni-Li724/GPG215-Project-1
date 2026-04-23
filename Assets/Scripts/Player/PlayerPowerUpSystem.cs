using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPowerUpSystem : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerShooter shooter;
    [SerializeField] private ShieldHealth  shield;
    [SerializeField] private BulletTypeSO  fireballBulletType;

    [SerializeField] private float triShotSpread      = 40f;
    [SerializeField] private float rapidFireMultiplier = 2.5f;
    private readonly List<PowerUpEffect> activeEffects = new List<PowerUpEffect>();
    public void Activate(PowerUpType type)
    {
        float duration  = GetDurationFromDB(type);
        PowerUpEffect effect = BuildEffect(type);
        if (effect == null) return;

        effect.Init(shooter, duration);
        effect.Apply();
        activeEffects.Add(effect);
        StartCoroutine(ExpireAfter(effect, duration));
    }
    public void ApplyFireStatusToEnemy(GameObject enemyGO)
    {
        ItemRow row = GameDatabase.Instance?.GetItem("fireball");
        
        Debug.Log($"ApplyFire called on {enemyGO.name}, row null: {row == null}, prefab null: {FireStatusEffect.FireParticlePrefab == null}");
    
        if (row == null) return;
        FireStatusEffect existing = enemyGO.GetComponent<FireStatusEffect>();
        if (existing != null) { existing.AddStack(); return; }
        FireStatusEffect effect = enemyGO.AddComponent<FireStatusEffect>();
        effect.Init(row.fire_dps, row.fire_spread_radius, row.fire_stack_multiplier);
    }

    private IEnumerator ExpireAfter(PowerUpEffect effect, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        effect.Remove();
        activeEffects.Remove(effect);

        // only clear shooter overrides when NO shooter effects remain
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
        if (GameDatabase.Instance == null || !GameDatabase.Instance.IsReady) return 5f;
        ItemRow row = GameDatabase.Instance.GetItem(type.ToString().ToLower());
        return row != null ? row.duration : 5f;
    }
}