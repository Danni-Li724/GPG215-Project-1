using System.Collections.Generic;
using UnityEngine;

// Applies DLC skin overrides to enemies and boss when they spawn.
// Fully generic — works for any enemy type via GetType().Name lookup
// against the skin manifest. No hardcoded type names anywhere.
public class LevelSkinApplier : MonoBehaviour
{
    public static LevelSkinApplier Instance { get; private set; }

    private string       activePack;
    private bool         skinReady;

    // built from manifest for O(1) lookups
    private Dictionary<string, string> skinLookup = new Dictionary<string, string>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // Called by GameManager.BeginRun() with the current level's dlcPackId.
    // Empty packId = base game level, nothing loads.
    public void BeginLevel(string packId)
    {
        skinLookup.Clear();
        skinReady  = false;
        activePack = packId;

        if (string.IsNullOrEmpty(packId)) { skinReady = true; return; }

        if (DLCLoader.Instance == null)
        {
            Debug.LogWarning("LevelSkinApplier: DLCLoader not in scene");
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
                Debug.LogWarning($"LevelSkinApplier: pack '{packId}' failed to load");
                skinReady = true; // still mark ready so game doesn't hang
            }
        });
    }

    // Called by EnemyManager after Activate() — works for ANY enemy type
    public void ApplySkinToEnemy(GameObject enemyGO)
    {
        if (!skinReady || string.IsNullOrEmpty(activePack)) return;

        // walk up component list to find the most derived type name
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

    // Called by GameManager when boss spawns
    public void ApplySkinToBoss(GameObject bossGO)
    {
        if (!skinReady || string.IsNullOrEmpty(activePack)) return;

        // boss type key is "LevelBoss"
        if (skinLookup.TryGetValue("LevelBoss", out string assetName))
            ApplySprite(bossGO, assetName);
    }

    private void ApplySprite(GameObject go, string assetName)
    {
        Sprite sprite = DLCLoader.Instance?.GetSprite(activePack, assetName);
        if (sprite == null) return;

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
