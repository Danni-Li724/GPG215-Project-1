using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Reads from LevelMapSO instead of JSON/StreamingAssets.
// No asset bundles, no streaming — all sprites assigned directly in SO.
// Pools prewarmed one instance per frame to avoid frame spikes.
public class ProceduralMapGeneratorLocal : MonoBehaviour
{
    [System.Serializable]
    public class NodeTypeConfig
    {
        public string nodeType;      // matches LevelMapSO.MapNodeEntry.nodeType
        public int    prewarmCount = 3;
    }

    [Header("Refs")]
    [SerializeField] private MileageSystem mileageSystem;
    [SerializeField] private Transform     nodeParent;

    [Header("Node Type Prewarm Counts")]
    [SerializeField] private List<NodeTypeConfig> nodeConfigs = new List<NodeTypeConfig>
    {
        new NodeTypeConfig { nodeType = "planet_rocky",  prewarmCount = 3 },
        new NodeTypeConfig { nodeType = "nebula_purple", prewarmCount = 3 },
        new NodeTypeConfig { nodeType = "nebula_blue",   prewarmCount = 3 },
        new NodeTypeConfig { nodeType = "asteroid",      prewarmCount = 4 },
        new NodeTypeConfig { nodeType = "planet_gas",    prewarmCount = 2 },
        new NodeTypeConfig { nodeType = "debris",        prewarmCount = 3 },
    };

    [Header("Spawn Settings")]
    [SerializeField] private Vector2 spawnXRange     = new Vector2(-2.5f, 2.5f);
    [SerializeField] private float   spawnY          = 8f;
    [SerializeField] private float   nodeScrollSpeed = 1.5f;
    [SerializeField] private float   spawnCooldown   = 0.4f;

    [Header("Asteroid")]
    [SerializeField] private float asteroidSpeedMin = 0.8f;
    [SerializeField] private float asteroidSpeedMax = 1.8f;
    [SerializeField] private float asteroidAngleMin = 10f;
    [SerializeField] private float asteroidAngleMax = 40f;

    // runtime
    private LevelMapSO    currentMap;
    private int           nextNodeIndex;
    private bool          isReady;
    private bool          enabled_ = true;
    private float         cooldownTimer;
    private const float   DespawnY = -12f;

    private readonly Dictionary<string, Queue<PooledNode>> pools      = new Dictionary<string, Queue<PooledNode>>();
    private readonly List<PooledNode>                       activeNodes = new List<PooledNode>();

    private class PooledNode
    {
        public GameObject     root;
        public SpriteRenderer sr;
        public string         nodeType;
        public bool           isAsteroid;
        public Vector2        velocity;
    }

    // ── Public API ────────────────────────────────────────────────────────

    public void BeginLevel(LevelMapSO map)
    {
        currentMap    = map;
        nextNodeIndex = 0;
        cooldownTimer = 0f;
        isReady       = false;
        ReturnAllToPool();
        StartCoroutine(PrewarmThenReady());
    }

    public void SetEnabled(bool value)
    {
        enabled_ = value;
        foreach (var n in activeNodes)
            if (n.root != null) n.root.SetActive(value);
    }

    // ── Init ──────────────────────────────────────────────────────────────

    private IEnumerator PrewarmThenReady()
    {
        // prewarm one instance per frame — no spike
        foreach (var cfg in nodeConfigs)
        {
            if (!pools.ContainsKey(cfg.nodeType))
                pools[cfg.nodeType] = new Queue<PooledNode>();

            for (int i = 0; i < cfg.prewarmCount; i++)
            {
                pools[cfg.nodeType].Enqueue(CreateNode(cfg.nodeType));
                yield return null;
            }
        }

        if (currentMap == null)
        {
            Debug.LogWarning("ProceduralMapGeneratorLocal: no LevelMapSO assigned");
            yield break;
        }

        // sort nodes by spawnDistance
        currentMap.nodes.Sort((a, b) => a.spawnDistance.CompareTo(b.spawnDistance));
        isReady = true;
        Debug.Log($"ProceduralMapGeneratorLocal: ready — {currentMap.nodes.Count} nodes");
    }

    // ── Update ────────────────────────────────────────────────────────────

    private void Update()
    {
        if (!enabled_ || !isReady || mileageSystem == null) return;
        CheckSpawn();
        ScrollAndCull();
    }

    private void CheckSpawn()
    {
        if (currentMap == null || nextNodeIndex >= currentMap.nodes.Count) return;
        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer > 0f) return;

        LevelMapSO.MapNodeEntry def = currentMap.nodes[nextNodeIndex];
        if (mileageSystem.CurrentMiles < def.spawnDistance) return;

        SpawnNode(def);
        nextNodeIndex++;
        cooldownTimer = spawnCooldown;
    }

    private void SpawnNode(LevelMapSO.MapNodeEntry def)
    {
        PooledNode node = GetFromPool(def.nodeType);
        if (node == null) return;

        float x = Random.Range(spawnXRange.x, spawnXRange.y);
        node.root.transform.position   = new Vector3(x, spawnY, 0f);
        node.root.transform.localScale = Vector3.one * Random.Range(def.scaleMin, def.scaleMax);
        node.sr.sortingOrder           = def.layer;
        node.isAsteroid                = false;
        node.velocity                  = Vector2.zero;

        // assign sprite
        if (def.sprites != null && def.sprites.Length > 0)
        {
            int idx = def.sprites.Length > 1
                ? Random.Range(0, def.sprites.Length)
                : 0;
            node.sr.sprite = def.sprites[idx];
        }

        // alpha
        Color c = Color.white;
        c.a = Mathf.Clamp01(def.alpha);
        node.sr.color = c;

        // asteroid diagonal movement
        if (def.nodeType == "asteroid")
        {
            node.isAsteroid = true;
            float angle = Random.Range(asteroidAngleMin, asteroidAngleMax);
            float dirX  = Random.value > 0.5f ? 1f : -1f;
            node.velocity = new Vector2(
                dirX * Mathf.Cos(angle * Mathf.Deg2Rad),
                -Mathf.Sin(angle * Mathf.Deg2Rad)
            ) * Random.Range(asteroidSpeedMin, asteroidSpeedMax);
        }

        node.root.SetActive(true);
        activeNodes.Add(node);
    }

    private void ScrollAndCull()
    {
        float dt = Time.deltaTime;
        for (int i = activeNodes.Count - 1; i >= 0; i--)
        {
            PooledNode n = activeNodes[i];
            if (n.root == null) { activeNodes.RemoveAt(i); continue; }

            n.root.transform.position += n.isAsteroid
                ? (Vector3)(n.velocity * dt)
                : Vector3.down * nodeScrollSpeed * dt;

            float px = n.root.transform.position.x;
            float py = n.root.transform.position.y;
            if (py < DespawnY || Mathf.Abs(px) > 10f)
            {
                ReturnToPool(n);
                activeNodes.RemoveAt(i);
            }
        }
    }

    // ── Pool ──────────────────────────────────────────────────────────────

    private PooledNode GetFromPool(string key)
    {
        if (pools.TryGetValue(key, out Queue<PooledNode> q) && q.Count > 0)
            return q.Dequeue();
        return CreateNode(key);
    }

    private PooledNode CreateNode(string key)
    {
        GameObject root = new GameObject($"Node_{key}");
        root.transform.SetParent(nodeParent != null ? nodeParent : transform, false);
        root.SetActive(false);
        SpriteRenderer sr = root.AddComponent<SpriteRenderer>();
        return new PooledNode { root = root, sr = sr, nodeType = key };
    }

    private void ReturnToPool(PooledNode node)
    {
        if (node.root == null) return;
        node.root.SetActive(false);
        node.root.transform.localScale = Vector3.one;
        node.sr.sprite  = null;
        node.sr.color   = Color.white;
        node.isAsteroid = false;
        node.velocity   = Vector2.zero;

        if (!pools.ContainsKey(node.nodeType))
            pools[node.nodeType] = new Queue<PooledNode>();
        pools[node.nodeType].Enqueue(node);
    }

    private void ReturnAllToPool()
    {
        foreach (var n in activeNodes) ReturnToPool(n);
        activeNodes.Clear();
    }
}
