using UnityEngine;

public class FireStatusEffectSettings : MonoBehaviour
{
    [SerializeField] private ParticleSystem fireParticlePrefab;

    private void Awake()
    {
        FireStatusEffect.FireParticlePrefab = fireParticlePrefab;
    }
}
