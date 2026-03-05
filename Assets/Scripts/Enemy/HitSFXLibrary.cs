using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Hit SFX Library", fileName = "HitSFXLibrary")]
public class HitSFXLibrary : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public HitVFXType type;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    public List<Entry> entries = new List<Entry>();
    public bool TryGet(HitVFXType type, out AudioClip clip, out float volume)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].type == type)
            {
                clip = entries[i].clip;
                volume = entries[i].volume;
                return clip != null;
            }
        }

        clip = null;
        volume = 1f;
        return false;
    }
}