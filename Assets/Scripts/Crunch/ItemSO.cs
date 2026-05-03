using UnityEngine;

[CreateAssetMenu(menuName = "Game/Item Stats", fileName = "ItemStats")]
public class ItemSO : ScriptableObject
{
    [Header("Identity")]
    public string itemKey; 

    [Header("Duration")]
    public float duration = 8f;

    [Header("Fireball Only")]
    public float fireDPS             = 0f;
    public float fireSpreadRadius    = 0f;
    public float fireStackMultiplier = 0f;
    public GameObject fireEffectPrefab;
}
