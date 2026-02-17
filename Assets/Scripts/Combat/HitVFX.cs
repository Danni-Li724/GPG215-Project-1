using UnityEngine;

public class HitVFX : MonoBehaviour
{
    private ParticleSystem hitParticles;
    private HitVFXPoolManager poolManager;
    private HitVFXType poolType;

    private void Awake()
    {
        hitParticles = GetComponent<ParticleSystem>();
    }

    public void Init(HitVFXPoolManager pool, HitVFXType type)
    {
        poolManager = pool;
        poolType = type;
    }

    public void PlayAt(Vector3 position)
    {
        transform.position = position;

        // stop and restart again
        hitParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        hitParticles.Play(true);

        gameObject.SetActive(true);
    }
    private void OnParticleSystemStopped()
    {
        if (poolManager != null)
            poolManager.Return(poolType, this);
        else
            gameObject.SetActive(false);
    }
}
