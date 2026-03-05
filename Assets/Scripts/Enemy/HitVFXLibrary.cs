using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "VFX/Hit VFX Library", fileName = "VFXLibrary")]
public class HitVFXLibrary : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public HitVFXType type;
        public HitVFX prefab;
        public int prewarmCount = 8;
    }

    public List<Entry> entries = new List<Entry>();

    public bool TryGetPrefab(HitVFXType type, out HitVFX prefab, out int prewarm)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].type == type)
            {
                prefab = entries[i].prefab;
                prewarm = entries[i].prewarmCount;
                return prefab != null;
            }
        }

        prefab = null;
        prewarm = 0;
        return false;
    }
}