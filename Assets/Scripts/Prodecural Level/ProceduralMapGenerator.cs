using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class ProceduralMapGenerator : MonoBehaviour, ITickable
{
    [System.Serializable]
    public class NodeTypeConfig
    {
        public string nodeType;           
        public int    prewarmCount = 3;  
        // public string particlePrefabKey; 
    }

    [Header("Refs")]
    [SerializeField] private MileageSystem mileageSystem;
    [SerializeField] private Transform     nodeParent;

    [Header("Spawn Settings")]
    [SerializeField] private float pixelsPerUnit = 100f;
    [SerializeField] private Vector2 spawnXRange = new Vector2(-2.5f, 2.5f);
    [SerializeField] private float spawnY = 8f;
    [SerializeField] private float nodeScrollSpeed = 1.5f;
    private string spritesFolder = "Maps/Sprites";

    // [Header("Node Type Configs")]
    // [SerializeField] private List<NodeTypeConfig> nodeConfigs = new List<NodeTypeConfig>
    // {
    //     new NodeTypeConfig { nodeType = "planet_rocky",  prewarmCount = 3, particlePrefabKey = "RockyRing"       },
    //     new NodeTypeConfig { nodeType = "nebula_purple", prewarmCount = 3, particlePrefabKey = "NebulaPurpleRing" },
    //     new NodeTypeConfig { nodeType = "nebula_blue",   prewarmCount = 3, particlePrefabKey = "NebulaBlueRing"   },
    //     new NodeTypeConfig { nodeType = "asteroid",      prewarmCount = 4, particlePrefabKey = "AsteroidTrail"    },
    //     new NodeTypeConfig { nodeType = "planet_gas",    prewarmCount = 2, particlePrefabKey = "GasRing"          },
    //     new NodeTypeConfig { nodeType = "debris",        prewarmCount = 3, particlePrefabKey = ""                 },
    // };
    
    [SerializeField] private List<NodeTypeConfig> nodeConfigs = new List<NodeTypeConfig>
    {
        new NodeTypeConfig { nodeType = "planet_rocky",  prewarmCount = 3 },
        new NodeTypeConfig { nodeType = "nebula_purple", prewarmCount = 3 },
        new NodeTypeConfig { nodeType = "nebula_blue",   prewarmCount = 3 },
        new NodeTypeConfig { nodeType = "asteroid",      prewarmCount = 4 },
        new NodeTypeConfig { nodeType = "planet_gas",    prewarmCount = 2 },
        new NodeTypeConfig { nodeType = "debris",        prewarmCount = 3 },
    };

    [Header("Asteroid Movement")]
    [SerializeField] private float asteroidSpeedMin = 0.8f;
    [SerializeField] private float asteroidSpeedMax = 1.8f;
    [SerializeField] private float asteroidAngleMin = 10f;  // degrees below horizontal
    [SerializeField] private float asteroidAngleMax = 40f;

    // [Header("Bundle")]
    // [SerializeField] private string bundleFolderPath = "NodeBundles";
    // [SerializeField] private string bundleName       = "nodeparticles";
    //
    private MapLayoutData layout;
    private int nextNodeIndex;
    private bool enabled_ = true;
    private bool isReady  = false;

    // private AssetBundle particleBundle;
    private readonly Dictionary<string, Queue<NodeInstance>> pools = new Dictionary<string, Queue<NodeInstance>>();
    // private readonly Dictionary<string, GameObject> particlePrefabs = new Dictionary<string, GameObject>();

    private readonly List<NodeInstance> activeNodes = new List<NodeInstance>();
    private const float DespawnY = -12f;

    private float spawnCooldown;
    private const float SpawnCooldownSeconds = 0.4f;
    
    private class NodeInstance
    {
        public GameObject root;
        public SpriteRenderer sr;
        public ParticleSystem particles;
        public string nodeType;
        public bool isAsteroid;
        public Vector2 asteroidVelocity;
    }
    
    private void Start()
    {
        // if (GameManager.instance != null)
        //     GameManager.instance.RegisterTickable(this);
    }

    private void OnDestroy()
    {
        // if (particleBundle != null)
        // {
        //     particleBundle.Unload(false);
        //     particleBundle = null;
        // }
        // if (GameManager.instance != null)
        //     GameManager.instance.UnregisterTickable(this);
    }

    public void SetEnabled(bool value)
    {
        enabled_ = value;
        foreach (var n in activeNodes)
            if (n.root != null) n.root.SetActive(value);
    }

    public void BeginLevel(int levelId, string spritesSubfolder = "Level1")
    {
        spritesFolder = $"Maps/Sprites/{spritesSubfolder}"; // better folder structure
        nextNodeIndex = 0;
        spawnCooldown = 0f;
        isReady       = false;

        ReturnAllToPool();
        StartCoroutine(LoadBundleThenLayout(levelId));
    }

    private IEnumerator LoadBundleThenLayout(int levelId)
    {
        // // load bundle if not already loaded
        // if (particleBundle == null)
        //     yield return StartCoroutine(LoadBundleRoutine());
        //
        // // cache particle prefabs from bundle
        // if (particleBundle != null)
        // {
        //     foreach (var cfg in nodeConfigs)
        //     {
        //         if (string.IsNullOrEmpty(cfg.particlePrefabKey)) continue;
        //         if (particlePrefabs.ContainsKey(cfg.particlePrefabKey)) continue;
        //
        //         GameObject prefab = particleBundle.LoadAsset<GameObject>(cfg.particlePrefabKey);
        //         if (prefab != null)
        //             particlePrefabs[cfg.particlePrefabKey] = prefab;
        //         else
        //             Debug.LogWarning($"prefab '{cfg.particlePrefabKey}' not found in bundle");
        //     }
        // }

        // prewarm pools
       
            foreach (var cfg in nodeConfigs)
                yield return StartCoroutine(PrewarmPoolAsync(cfg));

            yield return StartCoroutine(LoadLayoutRoutine(levelId));
        
    }

    // private IEnumerator LoadBundleRoutine()
    // {
    //     string path = System.IO.Path.Combine(Application.streamingAssetsPath, bundleFolderPath, bundleName);
    //
    //     using UnityWebRequest req = UnityWebRequestAssetBundle.GetAssetBundle(path);
    //     yield return req.SendWebRequest();
    //
    //     if (req.result != UnityWebRequest.Result.Success)
    //     {
    //         Debug.LogWarning($"bundle load failed: {req.error} at {path}");
    //         yield break;
    //     }
    //
    //     particleBundle = DownloadHandlerAssetBundle.GetContent(req);
    //     if (particleBundle == null)
    //         Debug.LogWarning("bundle was null after download");
    //     else
    //         Debug.Log("particle bundle loaded");
    // }

    private IEnumerator LoadLayoutRoutine(int levelId)
    {
        string path = $"Maps/level_map_{levelId}.json";
        string json = null;
        yield return StartCoroutine(StreamingAssets.LoadText(path, t => json = t));

        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.LogWarning($"no map at {path}");
            yield break;
        }

        try   { layout = JsonUtility.FromJson<MapLayoutData>(json); }
        catch (System.Exception e)
        {
            Debug.LogWarning($"JSON parse failed: {e.Message}");
            yield break;
        }

        layout.nodes.Sort((a, b) => a.spawnDistance.CompareTo(b.spawnDistance));
        isReady = true;
        Debug.Log($"ready: {layout.nodes.Count} nodes for level {levelId}");
    }

    
    // pooling
    // private void PrewarmPool(NodeTypeConfig cfg)
    // {
    //     if (!pools.ContainsKey(cfg.nodeType))
    //         pools[cfg.nodeType] = new Queue<NodeInstance>();
    //
    //     for (int i = 0; i < cfg.prewarmCount; i++)
    //         pools[cfg.nodeType].Enqueue(CreateNodeInstance(cfg));
    // }
    
    private IEnumerator PrewarmPoolAsync(NodeTypeConfig cfg)
    {
        if (!pools.ContainsKey(cfg.nodeType))
            pools[cfg.nodeType] = new Queue<NodeInstance>();
        for (int i = 0; i < cfg.prewarmCount; i++)
        {
            pools[cfg.nodeType].Enqueue(CreateNodeInstance(cfg));
            yield return null;
        }
    }

    private NodeInstance CreateNodeInstance(NodeTypeConfig cfg)
    {
        GameObject root = new GameObject($"Node_{cfg.nodeType}");
        root.transform.SetParent(nodeParent != null ? nodeParent : transform, false);
        root.SetActive(false);

        SpriteRenderer sr = root.AddComponent<SpriteRenderer>();

        // ParticleSystem ps = null;
        // if (!string.IsNullOrEmpty(cfg.particlePrefabKey) &&
        //     particlePrefabs.TryGetValue(cfg.particlePrefabKey, out GameObject prefab))
        // {
        //     GameObject psGO = Instantiate(prefab, root.transform);
        //     psGO.transform.localPosition = Vector3.zero;
        //     ps = psGO.GetComponent<ParticleSystem>();
        //     if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        // }
        //
        // return new NodeInstance { root = root, sr = sr, particles = ps, nodeType = cfg.nodeType };
        return new NodeInstance { root = root, sr = sr, nodeType = cfg.nodeType };
    }

    private NodeInstance GetFromPool(string nodeType)
    {
        if (pools.TryGetValue(nodeType, out Queue<NodeInstance> queue) && queue.Count > 0)
            return queue.Dequeue();

        // if pool exhausted just create a new one
        NodeTypeConfig cfg = nodeConfigs.Find(c => c.nodeType == nodeType);
        if (cfg != null) return CreateNodeInstance(cfg);

 
        GameObject root = new GameObject($"Node_{nodeType}");
        root.transform.SetParent(nodeParent != null ? nodeParent : transform, false);
        root.SetActive(false);
        SpriteRenderer sr = root.AddComponent<SpriteRenderer>();
        return new NodeInstance { root = root, sr = sr, nodeType = nodeType };
    }

    private void ReturnToPool(NodeInstance node)
    {
        if (node.root == null) return;

        node.root.SetActive(false);
        node.root.transform.localScale = Vector3.one;
        node.isAsteroid        = false;
        node.asteroidVelocity  = Vector2.zero;
        node.sr.sprite         = null;

        if (node.particles != null)
            node.particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (!pools.ContainsKey(node.nodeType))
            pools[node.nodeType] = new Queue<NodeInstance>();

        pools[node.nodeType].Enqueue(node);
    }

    private void ReturnAllToPool()
    {
        foreach (var n in activeNodes)
            ReturnToPool(n);
        activeNodes.Clear();
    }

    public void Update()
    {
        if (!enabled_ || !isReady || mileageSystem == null) return;
        CheckAndSpawnNodes(Time.deltaTime);
        ScrollAndCullNodes(Time.deltaTime);
    }

    private void CheckAndSpawnNodes(float dt)
    {
        if (layout == null || nextNodeIndex >= layout.nodes.Count) return;
        spawnCooldown -= dt;
        if (spawnCooldown > 0f) return;
        if (mileageSystem.CurrentMiles >= layout.nodes[nextNodeIndex].spawnDistance)
        {
            SpawnNode(layout.nodes[nextNodeIndex]);
            nextNodeIndex++;
            spawnCooldown = SpawnCooldownSeconds;
        }
    }

    private void ScrollAndCullNodes(float dt)
    {
        for (int i = activeNodes.Count - 1; i >= 0; i--)
        {
            NodeInstance n = activeNodes[i];
            if (n.root == null) { activeNodes.RemoveAt(i); continue; }

            if (n.isAsteroid)
                n.root.transform.position += (Vector3)(n.asteroidVelocity * dt);
            else
                n.root.transform.position += Vector3.down * nodeScrollSpeed * dt;

            float posY = n.root.transform.position.y;
            float posX = n.root.transform.position.x;
            if (posY < DespawnY || Mathf.Abs(posX) > 10f)
            {
                ReturnToPool(n);
                activeNodes.RemoveAt(i);
            }
        }
    }
    
    private void SpawnNode(MapNodeDefinition def)
    {
        NodeInstance node = GetFromPool(def.nodeType);

        // positions
        float x = Random.Range(spawnXRange.x, spawnXRange.y);
        node.root.transform.position   = new Vector3(x, spawnY, 0f);
        node.root.transform.localScale = Vector3.one * Random.Range(def.scaleMin, def.scaleMax);
        node.sr.sortingOrder           = def.layer;

        // type-specific setup before activation
        ConfigureNodeType(node, def);

        node.root.SetActive(true);

        // start particles
        if (node.particles != null)
        {
            node.particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            node.particles.Play(true);
        }

        activeNodes.Add(node);

        // load sprite async
        string spritePath = def.variationCount > 1
            ? $"{spritesFolder}/{def.spriteKey}_{Random.Range(0, def.variationCount)}.png"
            : $"{spritesFolder}/{def.spriteKey}.png";

        StartCoroutine(StreamingAssets.LoadSprite(spritePath, pixelsPerUnit, sprite =>
        {
            if (node.root != null && node.root.activeSelf)
                node.sr.sprite = sprite;
        }));
    }

    private void ConfigureNodeType(NodeInstance node, MapNodeDefinition def)
    {
        node.isAsteroid = false;

        switch (def.nodeType)
        {
            case "asteroid":
                node.isAsteroid = true;
                float angle = Random.Range(asteroidAngleMin, asteroidAngleMax);
                float dirX  = Random.value > 0.5f ? 1f : -1f;
                Vector2 dir = new Vector2(dirX * Mathf.Cos(angle * Mathf.Deg2Rad),
                    -Mathf.Sin(angle * Mathf.Deg2Rad));
                float speed = Random.Range(asteroidSpeedMin, asteroidSpeedMax);
                node.asteroidVelocity = dir * speed;

                if (node.particles != null)
                {
                    float trailAngle = Mathf.Atan2(-dir.y, -dir.x) * Mathf.Rad2Deg;
                    node.particles.transform.localRotation = Quaternion.Euler(0f, 0f, trailAngle);
                }
                break;
        }
    }

    private void ScrollAndCullNodes()
    {
        for (int i = activeNodes.Count - 1; i >= 0; i--)
        {
            NodeInstance n = activeNodes[i];
            if (n.root == null) { activeNodes.RemoveAt(i); continue; }

            if (n.isAsteroid)
            {
                // asteroids move in their own diagonal direction
                n.root.transform.position += (Vector3)(n.asteroidVelocity * Time.deltaTime);
            }
            else
            {
                // everything else scrolls straight down
                n.root.transform.position += Vector3.down * nodeScrollSpeed * Time.deltaTime;
            }

            // cull when off screen
            float posY = n.root.transform.position.y;
            float posX = n.root.transform.position.x;
            bool offBottom = posY < DespawnY;
            bool offSide   = Mathf.Abs(posX) > 10f; // asteroids can exit sideways

            if (offBottom || offSide)
            {
                ReturnToPool(n);
                activeNodes.RemoveAt(i);
            }
        }
    }
    
}