using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class HitVFXPoolManager : MonoBehaviour, ITickable
{
    [FormerlySerializedAs("library")]
    [Header("VFX")]
    [SerializeField] private HitVFXLibrary vfxLibrary;

    private readonly Dictionary<HitVFXType, Queue<HitVFX>> pools =
        new Dictionary<HitVFXType, Queue<HitVFX>>();
    
    [Header("SFX (mapped to HitVFXType)")]
    [SerializeField] private HitSFXLibrary hitSfxLibrary;
    [SerializeField] private AudioSource hitSfxSource;
    [SerializeField] private bool playHitSfx = true;

    public static HitVFXPoolManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        Prewarm();
    }

    private void Prewarm()
    {
        if (vfxLibrary == null)
            return;

        for (int i = 0; i < vfxLibrary.entries.Count; i++)
        {
            HitVFXLibrary.Entry e = vfxLibrary.entries[i];
            if (e == null || e.prefab == null || e.type == HitVFXType.None)
                continue;

            EnsurePool(e.type);

            for (int j = 0; j < e.prewarmCount; j++)
            {
                HitVFX v = Instantiate(e.prefab, transform);
                v.Init(this, e.type);
                v.gameObject.SetActive(false);
                pools[e.type].Enqueue(v);
            }
        }
    }

    private void EnsurePool(HitVFXType type)
    {
        if (!pools.ContainsKey(type))
            pools[type] = new Queue<HitVFX>();
    }

    public void Spawn(HitVFXType type, Vector3 position)
    {
        if (type == HitVFXType.None)
            return;

        // VFX
        if (vfxLibrary != null)
        {
            HitVFX prefab;
            int prewarm;
            if (vfxLibrary.TryGetPrefab(type, out prefab, out prewarm))
            {
                EnsurePool(type);

                HitVFX vfx;
                if (pools[type].Count > 0)
                    vfx = pools[type].Dequeue();
                else
                {
                    vfx = Instantiate(prefab, transform);
                    vfx.Init(this, type);
                    vfx.gameObject.SetActive(false);
                }

                vfx.PlayAt(position);
            }
        }

        // SFX
        if (playHitSfx && hitSfxLibrary != null && hitSfxSource != null)
        {
            AudioClip clip;
            float volume;
            if (hitSfxLibrary.TryGet(type, out clip, out volume))
                hitSfxSource.PlayOneShot(clip, volume);
        }
    }

    public void Return(HitVFXType type, HitVFX vfx)
    {
        if (vfx == null)
            return;

        EnsurePool(type);
        vfx.gameObject.SetActive(false);
        pools[type].Enqueue(vfx);
    }
}
