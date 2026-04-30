using System.Collections.Generic;
using UnityEngine;

public class LevelSkinApplier : MonoBehaviour
{
    public static LevelSkinApplier Instance { get; private set; }

    private string       activePack;
    private bool         skinReady;
    private Dictionary<string, string> skinLookup = new Dictionary<string, string>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }
    
    public void BeginLevel(string packId)
    {
        skinLookup.Clear();
        skinReady  = false;
        activePack = packId;

        if (string.IsNullOrEmpty(packId)) { skinReady = true; return; }

        if (DLCLoader.Instance == null)
        {
            Debug.LogWarning("levelSkinApplier: DLCLoader not in scene");
            skinReady = true;
            return;
        }

        DLCLoader.Instance.LoadPack(packId, success =>
        {
            if (success)
            {
                BuildLookup(packId);
                skinReady = true;
            }
            else
            {
                Debug.LogWarning($"levelSkinApplier: pack '{packId}' failed to load");
                skinReady = true; 
            }
        });
    }
    
    public void ApplySkinToEnemy(GameObject enemyGO)
    {
        if (!skinReady || string.IsNullOrEmpty(activePack)) return;
        MonoBehaviour[] components = enemyGO.GetComponents<MonoBehaviour>();
        foreach (var c in components)
        {
            string typeKey = c.GetType().Name;
            if (skinLookup.TryGetValue(typeKey, out string assetName))
            {
                ApplySprite(enemyGO, assetName);
                return;
            }
        }
    }
    
    public void ApplySkinToBoss(GameObject bossGO)
    {
        if (!skinReady || string.IsNullOrEmpty(activePack)) return;
        if (skinLookup.TryGetValue("LevelBoss", out string assetName))
            ApplySprite(bossGO, assetName);
    }

    private void ApplySprite(GameObject go, string assetName)
    {
        Sprite sprite = DLCLoader.Instance?.GetSprite(activePack, assetName);
        if (sprite == null) return;
        if (sprite == null)
        {
            Debug.LogWarning($"LevelSkinApplier: sprite '{assetName}' not found in bundle '{activePack}' — check asset name matches bundle exactly");
            return;
        }
        SpriteRenderer sr = go.GetComponentInChildren<SpriteRenderer>();
        if (sr != null) sr.sprite = sprite;
    }

    private void BuildLookup(string packId)
    {
        skinLookup.Clear();
        Skins manifest = DLCLoader.Instance?.GetSkins(packId);
        if (manifest == null) return;

        foreach (var entry in manifest.skins)
        {
            if (!string.IsNullOrEmpty(entry.typeKey))
                skinLookup[entry.typeKey] = entry.assetName;
        }

        Debug.Log($"LevelSkinApplier: built skin lookup with {skinLookup.Count} entries for '{packId}'");
    }
}
