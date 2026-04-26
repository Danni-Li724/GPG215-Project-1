using System;
using System.Collections.Generic;

[Serializable]
public class MapLayoutData
{
    public int levelId;
    public List<MapNodeDefinition> nodes = new List<MapNodeDefinition>();
}

[Serializable]
public class MapNodeDefinition
{
    public string nodeType;       
    public int    spawnDistance; 
    public string spriteKey;      
    public int    variationCount; 
    public float  scaleMin;
    public float  scaleMax;
    public int    layer;         
}
