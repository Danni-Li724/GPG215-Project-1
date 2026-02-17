using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Bullet Type", fileName = "BulletType")]
public class BulletTypeSO : ScriptableObject
{
    public float speed = 12f;
    public float lifetime = 2f;
    public int damage = 1;
    public Sprite sprite;
    public float visualScale;
    public float hitRadius;
    public LayerMask hitMask;
}
