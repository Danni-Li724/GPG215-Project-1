using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Level Map", fileName = "LevelMap")]
public class LevelMapSO : ScriptableObject
{
    public List<MapNodeEntry> nodes = new List<MapNodeEntry>();

    [System.Serializable]
    public class MapNodeEntry
    {
        public string nodeType;
        public int spawnDistance;
        public Sprite[] sprites;
        public int layer = -2;
        [Range(0f, 1f)]
        public float alpha = 1f;
        public float scaleMin = 0.5f;
        public float scaleMax = 1f;
    }
}
