using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Ticked by GameManager. Randomly spawns powerups from a configured list,
// guaranteeing each type spawns at least once per level.
// Spawns from one of two spawn points chosen at random each time.
public class PowerUpSpawnSystem : MonoBehaviour, ITickable
{
    [System.Serializable]
    public class PowerUpEntry
    {
        public GameObject prefab;

        [Tooltip("Average spawns per minute for this powerup")]
        public float spawnsPerMinute = 4f;

        [HideInInspector] public float accumulator;
        [HideInInspector] public bool  hasSpawnedOnce;
    }

    [Header("Powerup List")]
    [SerializeField] private List<PowerUpEntry> powerUps = new List<PowerUpEntry>();

    [Header("Spawn Points (two — one picked at random each spawn)")]
    [SerializeField] private Transform spawnPointA;
    [SerializeField] private Transform spawnPointB;

    [Header("Timing")]
    [Tooltip("Minimum seconds between any two spawns regardless of rate")]
    [SerializeField] private float minSpawnInterval = 2f;

    [Tooltip("How long into the level before guaranteed once-each spawns begin")]
    [SerializeField] private float guaranteedSpawnDelay = 5f;

    private float elapsedTime;
    private float spawnCooldown;
    private bool  guaranteedPhaseComplete;

    // ── Called by GameManager at level start ──────────────────────────────
    public void ResetSpawner()
    {
        elapsedTime              = 0f;
        spawnCooldown            = 0f;
        guaranteedPhaseComplete  = false;

        foreach (var entry in powerUps)
        {
            entry.accumulator    = 0f;
            entry.hasSpawnedOnce = false;
        }

        StartCoroutine(GuaranteedSpawnRoutine());
    }

    // ── ITickable ─────────────────────────────────────────────────────────
    public void Tick(float dt)
    {
        elapsedTime   += dt;
        spawnCooldown -= dt;

        if (spawnCooldown > 0f) return;

        // accumulate for each powerup type
        for (int i = 0; i < powerUps.Count; i++)
        {
            PowerUpEntry entry = powerUps[i];
            if (entry.prefab == null) continue;

            entry.accumulator += (entry.spawnsPerMinute / 60f) * dt;

            if (entry.accumulator >= 1f)
            {
                entry.accumulator -= 1f;
                SpawnPowerUp(entry.prefab);
                spawnCooldown = minSpawnInterval;
                return; // one spawn per tick cycle to avoid bursts
            }
        }
    }

    // ── Guaranteed once-per-level spawn ───────────────────────────────────
    private IEnumerator GuaranteedSpawnRoutine()
    {
        yield return new WaitForSeconds(guaranteedSpawnDelay);

        foreach (var entry in powerUps)
        {
            if (entry.prefab == null) continue;
            if (!entry.hasSpawnedOnce)
            {
                SpawnPowerUp(entry.prefab);
                entry.hasSpawnedOnce = true;
                yield return new WaitForSeconds(minSpawnInterval);
            }
        }

        guaranteedPhaseComplete = true;
    }

    // ── Spawn ─────────────────────────────────────────────────────────────
    private void SpawnPowerUp(GameObject prefab)
    {
        Transform spawnPoint = PickSpawnPoint();
        if (spawnPoint == null)
        {
            Debug.LogWarning("PowerUpSpawnSystem: no spawn points assigned");
            return;
        }

        GameObject go = Instantiate(prefab, spawnPoint.position, Quaternion.identity);

        // mark as spawned once for guaranteed phase
        foreach (var entry in powerUps)
            if (entry.prefab == prefab) entry.hasSpawnedOnce = true;
    }

    private Transform PickSpawnPoint()
    {
        if (spawnPointA == null) return spawnPointB;
        if (spawnPointB == null) return spawnPointA;
        return Random.value > 0.5f ? spawnPointA : spawnPointB;
    }
}
