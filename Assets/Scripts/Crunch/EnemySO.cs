using UnityEngine;

[CreateAssetMenu(menuName = "Game/Enemy Stats", fileName = "EnemyStats")]
public class EnemySO : ScriptableObject
{
    [Header("Identity")]
    public string typeKey;

    [Header("Stats")]
    public int   maxHealth   = 3;
    public float speed       = 1f;
    public float dropRate    = 0.2f;
    public HitVFXType hitVFXType = HitVFXType.Default;
}
