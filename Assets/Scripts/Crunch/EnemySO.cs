using UnityEngine;

// Replaces the Enemy table in planetmirage.db.
// Assign one per enemy type. Reference from the enemy prefab directly.
[CreateAssetMenu(menuName = "Game/Enemy Stats", fileName = "EnemyStats")]
public class EnemySO : ScriptableObject
{
    [Header("Identity")]
    public string typeKey; // must match the C# class name e.g. "BobClasher"

    [Header("Stats")]
    public int   maxHealth   = 3;
    public float speed       = 1f;
    public float dropRate    = 0.2f;
    public int   hitVFXType  = 1;
}
