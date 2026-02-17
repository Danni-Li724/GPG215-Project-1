using System.Collections.Generic;
using UnityEngine;

public class HitVFXPoolManager : MonoBehaviour, ITickable
{
    [SerializeField] private HitVFXLibrary library;

    private readonly Dictionary<HitVFXType, Queue<HitVFX>> pools =
        new Dictionary<HitVFXType, Queue<HitVFX>>();

    public static HitVFXPoolManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        Prewarm();
    }

    private void Prewarm()
    {
        if (library == null)
            return;

        for (int i = 0; i < library.entries.Count; i++)
        {
            HitVFXLibrary.Entry e = library.entries[i];
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
        if (type == HitVFXType.None || library == null)
            return;

        HitVFX prefab;
        int prewarm;
        if (!library.TryGetPrefab(type, out prefab, out prewarm))
            return;

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

    public void Return(HitVFXType type, HitVFX vfx)
    {
        if (vfx == null)
            return;

        EnsurePool(type);
        vfx.gameObject.SetActive(false);
        pools[type].Enqueue(vfx);
    }
}
