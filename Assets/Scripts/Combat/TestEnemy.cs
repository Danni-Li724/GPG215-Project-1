using UnityEngine;

public class TestEnemy : MonoBehaviour, ITickable, IDamageable, IHitVFXGetter
{
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float speed = 1.0f;
    [SerializeField] private HitVFXType hitVFXType = HitVFXType.Spark;
    public HitVFXType HitVFXType => hitVFXType;

    private int health;
    private bool isActive;
    private Transform target; 
    private EnemyPool pool;

    public bool IsActive => isActive;

    public void Activate(Vector2 position, Transform playerTarget, EnemyPool ownerPool)
    {
        transform.position = position;
        target = playerTarget;
        pool = ownerPool;

        health = maxHealth;
        isActive = true;
        gameObject.SetActive(true);
    }

    public void Tick(float dt)
    {
        if (!isActive || target == null)
            return;

        Vector2 pos = transform.position;
        Vector2 toPlayer = ((Vector2)target.position - pos);

        if (toPlayer.sqrMagnitude > 0.0001f)
        {
            Vector2 dir = toPlayer.normalized;
            pos += dir * speed * dt;
            transform.position = pos;
        }
    }

    public void TakeDamage(int amount)
    {
        if (!isActive)
            return;

        health -= amount;
        if (health <= 0)
            Deactivate();
    }
    private void Deactivate()
    {
        if (!isActive)
            return;

        isActive = false;
        gameObject.SetActive(false);

        if (pool != null)
            pool.Return(this);
    }
}
