using System.Collections.Generic;

[System.Serializable]
public class Skins
{
    // key = C# class name e.g. "BaseClasher", "LevelBoss"
    // value = asset name inside the bundle e.g. "enemy_void_clasher"
    public List<SkinEntry> skins = new List<SkinEntry>();

    [System.Serializable]
    public class SkinEntry
    {
        public string typeKey;   // matches GetType().Name
        public string assetName; // sprite asset name in the bundle
    }
}
