using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PowerUpSpawnSystem : MonoBehaviour, ITickable
{
    [System.Serializable]
    public class PowerUpEntry
    {
        public GameObject prefab;
        public float spawnsPerMinute = 4f;
        [HideInInspector] public float accumulator;
        [HideInInspector] public bool  hasSpawnedOnce;
    }

    [Header("Powerups")]
    [SerializeField] private List<PowerUpEntry> powerUps = new List<PowerUpEntry>();

    [Header("Spawn Points")]
    [SerializeField] private Transform spawnPointA;
    [SerializeField] private Transform spawnPointB;

    [Header("Timing")]
    [SerializeField] private float minSpawnInterval = 2f;
    [SerializeField] private float guaranteedSpawnDelay = 5f;

    private float elapsedTime;
    private float spawnCooldown;
    private bool  guaranteedPhaseComplete;
    
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
    public void Tick(float dt)
    {
        elapsedTime   += dt;
        spawnCooldown -= dt;

        if (spawnCooldown > 0f) return;
        
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
                return; 
            }
        }
    }
    
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
    
    private void SpawnPowerUp(GameObject prefab)
    {
        Transform spawnPoint = PickSpawnPoint();
        if (spawnPoint == null)
        {
            Debug.LogWarning("PowerUps: no spawn points assigned");
            return;
        }

        GameObject go = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
        
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
