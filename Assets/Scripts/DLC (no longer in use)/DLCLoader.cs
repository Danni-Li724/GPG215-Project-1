using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class DLCLoader : MonoBehaviour
{
    public static DLCLoader Instance { get; private set; }

    private const string DLCFolder = "DLC";
    // [SerializeField] private string bundleFileName = "level2";
    private readonly Dictionary<string, AssetBundle>  bundles   = new Dictionary<string, AssetBundle>(); // loaded bundles by packId
    private readonly Dictionary<string, Object>       assetCache = new Dictionary<string, Object>();
    private readonly Dictionary<string, Skins> skins  = new Dictionary<string, Skins>(); // skinsby packId
    private readonly HashSet<string> patchedPacks = new HashSet<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public void LoadPack(string packId, System.Action<bool> onComplete)
    {
        if (string.IsNullOrEmpty(packId)) { onComplete?.Invoke(true); return; }

        if (bundles.ContainsKey(packId))
        {
            onComplete?.Invoke(true); // already loaded
            return;
        }

        StartCoroutine(LoadPackRoutine(packId, onComplete));
    }

    public bool IsPackLoaded(string packId) => bundles.ContainsKey(packId);
    public Sprite GetSprite(string packId, string assetName)
    {
        if (string.IsNullOrEmpty(assetName)) return null;
        string key = $"{packId}/{assetName}";
        if (assetCache.TryGetValue(key, out Object cached)) return cached as Sprite;
        if (!bundles.TryGetValue(packId, out AssetBundle bundle)) return null;
        Sprite sprite = bundle.LoadAsset<Sprite>(assetName);
        if (sprite != null) assetCache[key] = sprite;
        return sprite;
    }

    public GameObject GetPrefab(string packId, string assetName)
    {
        if (string.IsNullOrEmpty(assetName)) return null;
        string key = $"{packId}/{assetName}";
        if (assetCache.TryGetValue(key, out Object cached)) return cached as GameObject;
        if (!bundles.TryGetValue(packId, out AssetBundle bundle)) return null;
        GameObject prefab = bundle.LoadAsset<GameObject>(assetName);
        if (prefab != null) assetCache[key] = prefab;
        return prefab;
    }
    
    public Skins GetSkins(string packId)
    {
        skins.TryGetValue(packId, out Skins m);
        return m;
    }

    public void UnloadPack(string packId)
    {
        if (bundles.TryGetValue(packId, out AssetBundle b))
        {
            b.Unload(false);
            bundles.Remove(packId);
        }
        skins.Remove(packId);

        List<string> toRemove = new List<string>();
        foreach (var key in assetCache.Keys)
            if (key.StartsWith(packId + "/")) toRemove.Add(key);
        foreach (var key in toRemove) assetCache.Remove(key);
    }

    // dlc pack loading:
    private IEnumerator LoadPackRoutine(string packId, System.Action<bool> onComplete)
    {
        string baseDir = Path.Combine(Application.streamingAssetsPath, DLCFolder, packId);

        // load asset bundle
        yield return StartCoroutine(LoadBundleRoutine(packId, baseDir));

        if (!bundles.ContainsKey(packId))
        {
            Debug.LogWarning($"bundle load failed for pack '{packId}'");
            onComplete?.Invoke(false);
            yield break;
        }

        // load skin manifest
        yield return StartCoroutine(LoadSkinRoutine(packId, baseDir));

        // apply SQL patch only once per session & per pack
        if (!patchedPacks.Contains(packId))
        {
            yield return StartCoroutine(ApplyPatchRoutine(packId, baseDir));
            patchedPacks.Add(packId);
        }

        Debug.Log($"pack '{packId}' ready");
        onComplete?.Invoke(true);
    }

    private IEnumerator LoadBundleRoutine(string packId, string baseDir)
    {
        string bundleName = packId.ToLower().Replace(" ", "");
        string path = Path.Combine(baseDir, bundleName); // * for scalability sake, look for file thats not 'patch.sql' or 'skins.json' rather than hardcoding bundle name
        
        Debug.Log($"DLCLoader: attempting bundle at path: {path}");
        Debug.Log($"DLCLoader: streamingAssetsPath = {Application.streamingAssetsPath}");
        // using UnityWebRequest req = UnityWebRequestAssetBundle.GetAssetBundle(path);
        // yield return req.SendWebRequest();
        
        string uri = new System.Uri(path).AbsoluteUri;

        Debug.Log($"DLCLoader: attempting bundle at path: {uri}");
    
        using UnityWebRequest req = UnityWebRequestAssetBundle.GetAssetBundle(uri);
        yield return req.SendWebRequest();
        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"bundle '{packId}' not found or failed: {req.error}");
            yield break;
        }
    
        AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(req);
        if (bundle != null)
        {
            bundles[packId] = bundle;
            Debug.Log($"bundle '{packId}' loaded ({bundle.GetAllAssetNames().Length} assets)");
        }
    }

    private IEnumerator LoadSkinRoutine(string packId, string baseDir)
    {
        string path = Path.Combine(baseDir, "skins.json");
        using UnityWebRequest req = UnityWebRequest.Get(path);
        yield return req.SendWebRequest();
    
        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.Log($"no skin.json for '{packId}' — no reskins applied");
            yield break;
        }
    
        string json = req.downloadHandler.text;
        try
        {
            Skins manifest = JsonUtility.FromJson<Skins>(json);
            if (manifest != null)
            {
                skins[packId] = manifest;
                Debug.Log($"skin loaded for '{packId}' ({manifest.skins.Count} skin entries)");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"skin parse failed for '{packId}': {e.Message}");
        }
    }
    
    
    private IEnumerator ApplyPatchRoutine(string packId, string baseDir)
    {
        string path = Path.Combine(baseDir, "patch.sql");
        using UnityWebRequest req = UnityWebRequest.Get(path);
        yield return req.SendWebRequest();
    
        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.Log($"no patch.sql for '{packId}' — skipping DB patch");
            yield break;
        }
    
        string sql = req.downloadHandler.text;
        if (string.IsNullOrWhiteSpace(sql)) yield break;
    
        if (GameDatabase.Instance == null)
        {
            Debug.LogWarning("sql GameDatabase not ready — cannot apply patch");
            yield break;
        }
    
        bool done = false;
        bool success = false;
    
        System.Threading.Tasks.Task.Run(() =>
        {
            success = GameDatabase.Instance.ExecutePatch(sql);
            done = true;
        });
    
        yield return new WaitUntil(() => done);
    
        if (success)
            Debug.Log($"sql patch applied for '{packId}'");
        else
            Debug.LogWarning($"sql patch failed for '{packId}'");
    }

    private void OnDestroy()
    {
        foreach (var b in bundles.Values)
            if (b != null) b.Unload(false);
        bundles.Clear();
        assetCache.Clear();
    }
}
