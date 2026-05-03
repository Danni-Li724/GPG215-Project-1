using UnityEngine;

public class BaseClasher : MonoBehaviour, ITickable, IDamageable, IHitVFXGetter, IPoolableEnemy, IDanger, IEnemyActivatable
{

    [Header("Stats Source")]
    [SerializeField] private EnemySO stats;
 
    [Header("Backup Stats")]
    [SerializeField] private int         maxHealth  = 3;
    [SerializeField] private float       speed      = 1.0f;
    [SerializeField] private HitVFXType  hitVFXType = HitVFXType.Default;
    public HitVFXType HitVFXType => hitVFXType;
 
    private int       health;
    private bool      isActive;
    private Transform target;
    private EnemyPool pool;
 
    public bool IsActive => isActive;
    public virtual void Activate(Vector2 position, Transform playerTarget, EnemyPool ownerPool)
    {
        transform.position = position;
        target             = playerTarget;
        pool               = ownerPool;
 
        if (stats != null)
        {
            maxHealth  = stats.maxHealth;
            speed      = stats.speed;
            hitVFXType = stats.hitVFXType; 
        }
 
        health   = maxHealth;
        isActive = true;
        gameObject.SetActive(true);
    }
 
    public virtual void Tick(float dt)
    {
        if (!isActive || target == null) return;
        Vector2 pos      = transform.position;
        Vector2 toPlayer = (Vector2)target.position - pos;
        if (toPlayer.sqrMagnitude > 0.0001f)
        {
            pos += toPlayer.normalized * speed * dt;
            transform.position = pos;
        }
    }
 
    public void TakeDamage(int amount)
    {
        if (!isActive) return;
        health -= amount;
        if (health <= 0) Deactivate();
    }
 
    private void Deactivate()
    {
        if (!isActive) return;
        isActive = false;
        gameObject.SetActive(false);
        pool?.Return(this);
    }
    public int PoolIndex { get; private set; }
    public void SetPoolIndex(int index) { PoolIndex = index; }
}
