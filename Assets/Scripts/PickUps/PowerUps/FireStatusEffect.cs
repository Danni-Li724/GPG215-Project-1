using System.Collections.Generic;
using UnityEngine;

public class FireStatusEffect : MonoBehaviour, ITickable
{
    public static ParticleSystem FireParticlePrefab;

    // global registry prevents spreading to already burning enemies,as this game is literally depleting my memory xD
    private static readonly HashSet<GameObject> burningObjects = new HashSet<GameObject>();

    private float dps;
    private float spreadRadius;
    private float stackMultiplier;
    private int   stackCount = 1;
    private float damageAccumulator;
    private float spreadCheckTimer;
    private const float SpreadCheckInterval = 1.0f; // check less frequently
    
    private const int MaxStacks = 5;
    
    private IDamageable host;
    private ParticleSystem fireParticle;

    public void Init(float dps, float spreadRadius, float stackMultiplier)
    {
        this.dps             = dps;
        this.spreadRadius    = spreadRadius;
        this.stackMultiplier = stackMultiplier;
        host = GetComponent<IDamageable>();

        // register this object as burning
        burningObjects.Add(gameObject);

        if (FireParticlePrefab != null)
        {
            fireParticle = Instantiate(FireParticlePrefab, transform.position,
                Quaternion.identity, transform);
            fireParticle.Play();
        }
    }

    public void AddStack()
    {
        stackCount = Mathf.Min(stackCount + 1, MaxStacks);
        if (fireParticle != null)
        {
            var main = fireParticle.main;
            main.simulationSpeed = 1f + (stackCount - 1) * 0.3f;
        }
    }
    
    private void Update()
    {
        Tick(Time.deltaTime); // temp update for now
    }

    public void Tick(float dt)
    {
        if (fireParticle != null)
            fireParticle.transform.position = transform.position;

        float effectiveDps = dps * Mathf.Pow(stackMultiplier, stackCount - 1);
        damageAccumulator += effectiveDps * dt;
        if (damageAccumulator >= 1f)
        {
            int dmg = Mathf.FloorToInt(damageAccumulator);
            damageAccumulator -= dmg;
            host?.TakeDamage(dmg);
        }

        spreadCheckTimer += dt;
        if (spreadCheckTimer >= SpreadCheckInterval)
        {
            spreadCheckTimer = 0f;
            TrySpreadFire();
        }
    }

    private void TrySpreadFire()
    {
        // only spread to enemies NOT already in the registry 
        Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, spreadRadius);
        foreach (var col in nearby)
        {
            if (col == null) continue;

            MonoBehaviour mb = col.GetComponentInParent<MonoBehaviour>();
            if (mb == null || mb.gameObject == gameObject) continue;

            IDamageable damageable = mb.GetComponent<IDamageable>();
            if (damageable == null) continue;

            // skip if already burning
            if (burningObjects.Contains(mb.gameObject)) continue;
            
            IPoolableEnemy enemy = mb.GetComponent<IPoolableEnemy>();
            if (enemy == null) continue;

            FireStatusEffect newEffect = mb.gameObject.AddComponent<FireStatusEffect>();
            newEffect.Init(dps, spreadRadius, stackMultiplier);
        }
    }

    private void OnDestroy()
    {
        burningObjects.Remove(gameObject);
        if (fireParticle != null) Destroy(fireParticle.gameObject);
    }
}