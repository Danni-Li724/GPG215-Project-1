using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SQLite4Unity3d;
using UnityEngine;
using UnityEngine.Networking;

public class GameDatabase : MonoBehaviour
{
    public static GameDatabase Instance { get; private set; }

    private SQLiteConnection db;
    public bool IsReady { get; private set; }

    private readonly Dictionary<string, EnemyStatsRow>      enemyCache = new();
    private readonly Dictionary<int,    LevelRow>           levelCache = new();
    private readonly Dictionary<string, ItemRow>            itemCache  = new();
    private readonly Dictionary<int,    List<LevelNodeRow>> nodeCache  = new();

    private const string DbName = "planetmirage.db";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        StartCoroutine(InitRoutine());
    }

    private IEnumerator InitRoutine()
    {
        string destPath = Path.Combine(Application.persistentDataPath, DbName);
        if (!File.Exists(destPath))
        {
            string srcPath = Path.Combine(Application.streamingAssetsPath, DbName);
            using UnityWebRequest req = UnityWebRequest.Get(srcPath);
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"could not read {DbName} from StreamingAssets: {req.error}. Starting with empty DB.");
            }
            else
            {
                    byte[] data = req.downloadHandler.data;
                    if (data == null || data.Length == 0)
                    {
                        Debug.LogWarning("downloaded DB was empty or null");
                        yield break;
                    }
                    File.WriteAllBytes(destPath, data);
            }
        }
        
        bool done = false;
        string error = null;

        Task.Run(() =>
        {
            try
            {
                db = new SQLiteConnection(destPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
                CreateTablesIfMissing();
                CacheAllTables();
            }
            catch (System.Exception e)
            {
                error = e.Message;
            }
            finally
            {
                done = true;
            }
        });

        yield return new WaitUntil(() => done);

        if (error != null)
            Debug.LogWarning($"init failed: {error}");
        else
        {
            IsReady = true;
            Debug.Log($"database ready — enemies:{enemyCache.Count} levels:{levelCache.Count} items:{itemCache.Count}");
        }
    }

    private void CreateTablesIfMissing()
    {
        db.CreateTable<EnemyStatsRow>();
        db.CreateTable<LevelRow>();
        db.CreateTable<ItemRow>();
        db.CreateTable<LevelNodeRow>();
        db.CreateTable<PlayerBestRow>();
    }

    private void CacheAllTables()
    {
        enemyCache.Clear();
        foreach (var row in db.Table<EnemyStatsRow>())
            enemyCache[row.type_key] = row;

        levelCache.Clear();
        foreach (var row in db.Table<LevelRow>())
            levelCache[row.id] = row;

        itemCache.Clear();
        foreach (var row in db.Table<ItemRow>())
            itemCache[row.item_key] = row;

        nodeCache.Clear();
        foreach (var row in db.Table<LevelNodeRow>())
        {
            if (!nodeCache.ContainsKey(row.level_id))
                nodeCache[row.level_id] = new List<LevelNodeRow>();
            nodeCache[row.level_id].Add(row);
        }
    }

    public EnemyStatsRow GetEnemy(string typeKey)
    {
        enemyCache.TryGetValue(typeKey, out var row);
        return row;
    }

    public LevelRow GetLevel(int id)
    {
        levelCache.TryGetValue(id, out var row);
        return row;
    }

    public ItemRow GetItem(string itemKey)
    {
        itemCache.TryGetValue(itemKey, out var row);
        return row;
    }

    public List<LevelNodeRow> GetNodesForLevel(int levelId)
    {
        nodeCache.TryGetValue(levelId, out var rows);
        return rows ?? new List<LevelNodeRow>();
    }
    public void SavePlayerBest(PlayerBestRow row)
    {
        if (db == null) return;
        db.Execute("DELETE FROM PlayerBest");
        db.Insert(row);
    }

    public PlayerBestRow LoadPlayerBest()
    {
        if (db == null) return null;
        return db.Table<PlayerBestRow>()
                 .OrderByDescending(r => r.total_mileage)
                 .FirstOrDefault();
    }


#if UNITY_EDITOR
    [ContextMenu("Reload Database Cache")]
    public void ReloadCache()
    {
        if (db == null) { Debug.LogWarning("DB not open"); return; }
        CacheAllTables();
        Debug.Log("cache reloaded");
    }
#endif

    private void OnDestroy()
    {
        db?.Close();
    }
}
