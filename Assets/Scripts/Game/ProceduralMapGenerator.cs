using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralMapGenerator : MonoBehaviour
{
    [SerializeField] private MileageSystem mileageSystem;
    [SerializeField] private Transform     nodeParent;     
    [SerializeField] private float         pixelsPerUnit = 100f;
    [SerializeField] private Vector2       spawnXRange   = new Vector2(-4f, 4f);
    [SerializeField] private float         spawnY        = 12f; 
    [SerializeField] private float         nodeScrollSpeed = 2f;

    private MapLayoutData layout;
    private int           nextNodeIndex = 0;
    private bool          enabled_      = true;
    private bool          isReady       = false;
    
    private readonly List<GameObject> activeNodes = new List<GameObject>();
    private const float DespawnY = -12f;
    private float spawnCooldown = 0f;
    private const float SpawnCooldownSeconds = 0.5f;

    public void SetEnabled(bool value)
    {
        enabled_ = value;
        // hide any nodes already spawned if toggled off
        if (!value)
        {
            foreach (var n in activeNodes)
                if (n != null) n.SetActive(false);
        }
        else
        {
            foreach (var n in activeNodes)
                if (n != null) n.SetActive(true);
        }
    }

    // Called by GameManager/LevelInfoPanel when a level begins, passing the SQL level id
    public void BeginLevel(int levelId)
    {
        nextNodeIndex = 0;
        foreach (var n in activeNodes)
            if (n != null) Destroy(n);
        activeNodes.Clear();

        isReady = false;
        StartCoroutine(LoadLayoutRoutine(levelId));
    }

    private IEnumerator LoadLayoutRoutine(int levelId)
    {
        string path = $"Maps/level_map_{levelId}.json";
        string json = null;
        yield return StartCoroutine(StreamingAssets.LoadText(path, t => json = t));

        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.LogWarning($"ProceduralMapGenerator: no map found at {path} — background will be empty");
            yield break;
        }

        try
        {
            layout = JsonUtility.FromJson<MapLayoutData>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"ProceduralMapGenerator: JSON parse failed: {e.Message}");
            yield break;
        }

        // sort ascending by spawnDistance
        layout.nodes.Sort((a, b) => a.spawnDistance.CompareTo(b.spawnDistance));
        isReady = true;
    }

    private void Update()
    {
        if (!enabled_ || !isReady || mileageSystem == null) return;

        CheckAndSpawnNodes();
        ScrollAndCullNodes();
    }
    private void CheckAndSpawnNodes()
    {
        if (layout == null || nextNodeIndex >= layout.nodes.Count) return;

        spawnCooldown -= Time.deltaTime;
        if (spawnCooldown > 0f) return;

        // only spawn one node per check to avoid infinit loop
        if (mileageSystem.CurrentMiles >= layout.nodes[nextNodeIndex].spawnDistance)
        {
            SpawnNode(layout.nodes[nextNodeIndex]);
            nextNodeIndex++;
            spawnCooldown = SpawnCooldownSeconds;
        }
    }

    private void SpawnNode(MapNodeDefinition def)
    {
        // pick variation suffix
        string spritePath = def.variationCount > 1
            ? $"Maps/Sprites/{def.spriteKey}_{Random.Range(0, def.variationCount)}.png"
            : $"Maps/Sprites/{def.spriteKey}.png";

        float x     = Random.Range(spawnXRange.x, spawnXRange.y);
        float scale = Random.Range(def.scaleMin, def.scaleMax);

        // create a placeholder GO at once and sprite loads async 
        GameObject go = new GameObject($"Node_{def.nodeType}");
        go.transform.SetParent(nodeParent, false);
        go.transform.position = new Vector3(x, spawnY, 0f);
        go.transform.localScale = Vector3.one * scale;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = def.layer;

        activeNodes.Add(go);

        // load sprite async 
        StartCoroutine(StreamingAssets.LoadSprite(spritePath, pixelsPerUnit, sprite =>
        {
            if (go != null && sprite != null)
                sr.sprite = sprite;
        }));
    }

    private void ScrollAndCullNodes()
    {
        for (int i = activeNodes.Count - 1; i >= 0; i--)
        {
            GameObject n = activeNodes[i];
            if (n == null) { activeNodes.RemoveAt(i); continue; }

            n.transform.position += Vector3.down * nodeScrollSpeed * Time.deltaTime;

            if (n.transform.position.y < DespawnY)
            {
                Destroy(n);
                activeNodes.RemoveAt(i);
            }
        }
    }
}
