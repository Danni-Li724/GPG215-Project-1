using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextureMapSpawner : MonoBehaviour
{
    [Header("Map")]
    [SerializeField] private SpriteRenderer mapSprite; 
    [SerializeField] private float pixelSampleStep = 50;

    [Header("Prefabs")]
    [SerializeField] private GameObject redPrefab; 
    [SerializeField] private GameObject bluePrefab;

    [Header("Spawn")]
    [SerializeField] private float spawnZ = 0f;     
    [SerializeField] private Transform spawnParent; 

    [Header("Colour Thresholds")]
    [SerializeField] private float orangeMinR = 0.6f;
    [SerializeField] private float orangeMaxG = 0.6f;
    [SerializeField] private float orangeMaxB = 0.2f;

    [SerializeField] private float greenMaxR  = 0.4f;
    [SerializeField] private float greenMinG  = 0.6f;
    [SerializeField] private float greenMaxB  = 0.4f;
    
    public void Spawn()
    {
        Texture2D tex = mapSprite.sprite?.texture;
        ClearSpawned();
        // get world-space bounds of the sprite
        Bounds bounds = mapSprite.bounds;
        int texW = tex.width;
        int texH = tex.height;

        int redCount  = 0;
        int blueCount = 0;

        for (int x = 0; x < texW; x += (int)pixelSampleStep)
        {
            for (int y = 0; y < texH; y += (int)pixelSampleStep)
            {
                Color pixel = tex.GetPixel(x, y);

                // skip transparent pixels
                if (pixel.a < 0.1f) continue;
                bool isOrange = pixel.r >= orangeMinR && pixel.g <= orangeMaxG && pixel.b <= orangeMaxB;
                bool isGreen  = pixel.r <= greenMaxR  && pixel.g >= greenMinG  && pixel.b <= greenMaxB;
                if (!isOrange && !isGreen) continue;
                // convert pixel coordinate to world position
                float normX = (float)x / texW;
                float normY = (float)y / texH;
                Vector3 worldPos = new Vector3(
                    Mathf.Lerp(bounds.min.x, bounds.max.x, normX),
                    Mathf.Lerp(bounds.min.y, bounds.max.y, normY),
                    spawnZ
                );

                if (isOrange)
                {
                    Instantiate(redPrefab, worldPos, Quaternion.identity, spawnParent);
                    redCount++;
                }
                else
                {
                    Instantiate(bluePrefab, worldPos, Quaternion.identity, spawnParent);
                    blueCount++;
                }
            }
        }
        mapSprite.enabled = false;
        Debug.Log($"spawned {redCount} red and {blueCount} blue ");
    }
    public void ClearSpawned()
    {
        mapSprite.enabled = true;
        if (spawnParent == null) return;
        for (int i = spawnParent.childCount - 1; i >= 0; i--)
            Destroy(spawnParent.GetChild(i).gameObject);
    }
}
