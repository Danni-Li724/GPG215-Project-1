using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralMapLocal : MonoBehaviour
{
    [System.Serializable]
    public class NodeType
    {
        public string   key;          
        public Sprite[] sprites;       
        public int      prewarmCount = 3;
    }
    
    [Header("Refs")]
    [SerializeField] private MileageSystem mileageSystem;
    [SerializeField] private Transform     nodeParent;

    [Header("Node Types")]
    [SerializeField] private List<NodeType> nodeTypes = new List<NodeType>();

    [Header("Spawn")]
    [SerializeField] private Vector2 spawnXRange     = new Vector2(-2.5f, 2.5f);
    [SerializeField] private float   spawnY          = 8f;
    [SerializeField] private float   nodeScrollSpeed = 1.5f;
    [SerializeField] private float   spawnCooldown   = 0.4f;

    [Header("Asteroid")]
    [SerializeField] private float asteroidSpeedMin = 0.8f;
    [SerializeField] private float asteroidSpeedMax = 1.8f;
    [SerializeField] private float asteroidAngleMin = 10f;
    [SerializeField] private float asteroidAngleMax = 40f;
    
    private MapLayoutData layout;
    private int           nextNodeIndex;
    private bool          isReady;
    private bool          enabled_ = true;
    private float         cooldownTimer;
    private const float   DespawnY = -20f;

    private readonly Dictionary<string, Queue<PooledNode>>  pools       = new Dictionary<string, Queue<PooledNode>>();
    private readonly Dictionary<string, NodeType>           typeLookup  = new Dictionary<string, NodeType>();
    private readonly List<PooledNode>                        activeNodes = new List<PooledNode>();

    private class PooledNode
    {
        public GameObject     root;
        public SpriteRenderer sr;
        public string         key;
        public bool           isAsteroid;
        public Vector2        velocity;
    }

    private void Awake()
    {
        foreach (var t in nodeTypes)
            if (!string.IsNullOrEmpty(t.key))
                typeLookup[t.key] = t;
    }

    private void Update()
    {
        if (!enabled_ || !isReady || mileageSystem == null) return;
        CheckSpawn();
        ScrollAndCull();
    }

    public void BeginLevel(int levelId, string spritesSubfolder = "Level1")
    {
        isReady       = false;
        nextNodeIndex = 0;
        cooldownTimer = 0f;
        ReturnAllToPool();
        StartCoroutine(InitRoutine(levelId));
    }

    public void SetEnabled(bool value)
    {
        enabled_ = value;
        foreach (var n in activeNodes)
            if (n.root != null) n.root.SetActive(value);
    }

    private IEnumerator InitRoutine(int levelId)
    {
        foreach (var t in nodeTypes)
        {
            if (!pools.ContainsKey(t.key))
                pools[t.key] = new Queue<PooledNode>();

            for (int i = 0; i < t.prewarmCount; i++)
            {
                pools[t.key].Enqueue(CreateNode(t.key));
                yield return null;
            }
        }
        
        string json = null;
        yield return StartCoroutine(StreamingAssets.LoadText($"Maps/level_map_{levelId}.json", t => json = t));

        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.LogWarning($"ProceduralMapLocal: no map found for level {levelId}");
            yield break;
        }

        try
        {
            layout = JsonUtility.FromJson<MapLayoutData>(json);
            layout.nodes.Sort((a, b) => a.spawnDistance.CompareTo(b.spawnDistance));
            isReady = true;
            Debug.Log($"ProceduralMapLocal: ready — {layout.nodes.Count} nodes");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"ProceduralMapLocal: JSON parse failed: {e.Message}");
        }
    }

    private void CheckSpawn()
    {
        if (layout == null || nextNodeIndex >= layout.nodes.Count) return;
        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer > 0f) return;

        MapNodeDefinition def = layout.nodes[nextNodeIndex];
        if (mileageSystem.CurrentMiles < def.spawnDistance) return;

        SpawnNode(def);
        nextNodeIndex++;
        cooldownTimer = spawnCooldown;
    }

    private void SpawnNode(MapNodeDefinition def)
    {
        PooledNode node = GetFromPool(def.nodeType);
        if (node == null) return;

        float x = Random.Range(spawnXRange.x, spawnXRange.y);
        node.root.transform.position   = new Vector3(x, spawnY, 0f);
        node.root.transform.localScale = Vector3.one * Random.Range(def.scaleMin, def.scaleMax);
        node.sr.sortingOrder           = def.layer;
        node.isAsteroid                = false;
        node.velocity                  = Vector2.zero;

   
        if (typeLookup.TryGetValue(def.nodeType, out NodeType t) && t.sprites != null && t.sprites.Length > 0)
        {
            int idx = def.variationCount > 1
                ? Random.Range(0, Mathf.Min(def.variationCount, t.sprites.Length))
                : 0;
            node.sr.sprite = t.sprites[idx];
        }
        
        Color c = Color.white;
        // c.a = Mathf.Clamp01(def.alpha);
        node.sr.color = c;

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
        return new PooledNode { root = root, sr = sr, key = key };
    }

    private void ReturnToPool(PooledNode node)
    {
        if (node.root == null) return;
        node.root.SetActive(false);
        node.root.transform.localScale = Vector3.one;
        node.sr.sprite = null;
        node.sr.color  = Color.white;
        node.isAsteroid = false;
        node.velocity   = Vector2.zero;

        if (!pools.ContainsKey(node.key))
            pools[node.key] = new Queue<PooledNode>();
        pools[node.key].Enqueue(node);
    }

    private void ReturnAllToPool()
    {
        foreach (var n in activeNodes)
            ReturnToPool(n);
        activeNodes.Clear();
    }
}
