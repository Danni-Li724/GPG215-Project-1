using UnityEngine;

public class Bullet : MonoBehaviour
{
    private BulletTypeSO type;
    private Vector2 velocity;
    private float lifeRemaining;
    private bool isActive;
    private SpriteRenderer spriteRenderer;
    public bool IsFireball { get; set; }
    public PlayerPowerUpSystem PowerUpSystem { get; set; }

    public bool IsActive => isActive;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Activate(BulletTypeSO bulletType, Vector2 startPos, Vector2 direction)
    {
        type = bulletType;
        transform.position = startPos;
        if (direction.sqrMagnitude < 0.0001f) direction = Vector2.up;
        direction.Normalize();
        velocity       = direction * type.speed;
        lifeRemaining  = type.lifetime;
        if (spriteRenderer != null && type.sprite != null)
            spriteRenderer.sprite = type.sprite;
        transform.localScale = Vector3.one * Mathf.Max(0.01f, type.visualScale);
        isActive = true;
        gameObject.SetActive(true);
    }

    public void Tick(float deltaTime)
    {
        if (!isActive) return;

        Vector3 pos = transform.position;
        pos.x += velocity.x * deltaTime;
        pos.y += velocity.y * deltaTime;
        transform.position = pos;

        lifeRemaining -= deltaTime;
        if (lifeRemaining <= 0f) { Deactivate(); return; }

        if (type.hitRadius > 0f)
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, type.hitRadius, type.hitMask);
            if (hit != null)
            {
                IDamageable damageable = hit.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(type.damage);
                    if (IsFireball && PowerUpSystem != null)
                    {
                        GameObject enemyGO = hit.GetComponentInParent<MonoBehaviour>()?.gameObject;
                        if (enemyGO != null)
                            PowerUpSystem.ApplyFireStatusToEnemy(enemyGO);
                    }

                    IHitVFXGetter vfxGetter = hit.GetComponent<IHitVFXGetter>();
                    if (vfxGetter != null && HitVFXPoolManager.Instance != null)
                        HitVFXPoolManager.Instance.Spawn(vfxGetter.HitVFXType, transform.position);
                }
                Deactivate();
            }
        }
    }

    public void Deactivate()
    {
        if (!isActive) return;
        isActive = false;
        IsFireball    = false;
        PowerUpSystem = null;
        gameObject.SetActive(false);
    }
}