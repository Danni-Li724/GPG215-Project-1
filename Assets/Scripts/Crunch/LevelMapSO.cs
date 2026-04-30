using System.Collections.Generic;
using UnityEngine;

// Replaces level_map_N.json in StreamingAssets/Maps/.
// One asset per level. Assign on LevelInfoSOLocal.
[CreateAssetMenu(menuName = "Game/Level Map", fileName = "LevelMap")]
public class LevelMapSO : ScriptableObject
{
    public List<MapNodeEntry> nodes = new List<MapNodeEntry>();

    [System.Serializable]
    public class MapNodeEntry
    {
        [Tooltip("Must match a key in ProceduralMapGeneratorLocal's node type list")]
        public string nodeType;

        [Tooltip("Cumulative mileage at which this node spawns")]
        public int spawnDistance;

        [Tooltip("Sprites to pick from — multiple = random variation")]
        public Sprite[] sprites;

        [Tooltip("Sorting layer order — negative = behind")]
        public int layer = -2;

        [Range(0f, 1f)]
        [Tooltip("Transparency — 1 = fully opaque, 0 = invisible")]
        public float alpha = 1f;

        public float scaleMin = 0.5f;
        public float scaleMax = 1f;
    }
}
