using System;
using System.Collections.Generic;

// Deserialized from StreamingAssets/Maps/level_map_X.json via JsonUtility.
// Designer-friendly: edit the JSON file, no recompile needed.
[Serializable]
public class MapLayoutData
{
    public int levelId;
    public List<MapNodeDefinition> nodes = new List<MapNodeDefinition>();
}

[Serializable]
public class MapNodeDefinition
{
    public string nodeType;       // "planet","debris","nebula","hazard"
    public int    spawnDistance;  // cumulative miles at which this node spawns
    public string spriteKey;      // filename under StreamingAssets/Maps/Sprites/
    public int    variationCount; // picks random 0..variationCount-1 suffix; 0 = no suffix
    public float  scaleMin;
    public float  scaleMax;
    public int    layer;          // used as sortingOrder offset
}
