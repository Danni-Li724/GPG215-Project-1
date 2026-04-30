using UnityEngine;

// Replaces the Item table in planetmirage.db.
// One asset per powerup type. Assign on PlayerPowerUpSystemLocal.
[CreateAssetMenu(menuName = "Game/Item Stats", fileName = "ItemStats")]
public class ItemSO : ScriptableObject
{
    [Header("Identity")]
    public string itemKey; // e.g. "trishot", "fireball"

    [Header("Duration")]
    public float duration = 8f;

    [Header("Fireball Only")]
    public float fireDPS             = 0f;
    public float fireSpreadRadius    = 0f;
    public float fireStackMultiplier = 0f;
    public GameObject fireEffectPrefab;
}
