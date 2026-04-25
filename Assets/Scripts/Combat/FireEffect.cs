using UnityEngine;
public class FireEffect : MonoBehaviour
{
    [SerializeField] private float duration = 5f;
    [SerializeField] private float dps      = 1f;

    private IDamageable host;
    private float damageAccumulator;
    private float lifeRemaining;
    private bool  spreading = true;
    private GameObject parentGO;

    private void Start()
    {
        host          = GetComponentInParent<IDamageable>();
        lifeRemaining = duration;
        // cache parent reference on start
        parentGO = transform.parent?.gameObject;
        GetComponent<ParticleSystem>().Play();
    }

    private void Update()
    {
        // if enemy dies and returns to pool, kill the effect so the enemy doesnt get reactivated with this effect still attached
        if (parentGO == null || !parentGO.activeInHierarchy)
        {
            Destroy(gameObject);
            return;
        }

        lifeRemaining -= Time.deltaTime;

        damageAccumulator += dps * Time.deltaTime;
        if (damageAccumulator >= 1f)
        {
            int damage = Mathf.FloorToInt(damageAccumulator);
            damageAccumulator -= damage;
            host?.TakeDamage(damage);
        }

        if (lifeRemaining <= 0f)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!spreading) return;

        IPoolableEnemy enemy = other.GetComponentInParent<IPoolableEnemy>();
        if (enemy == null) return;

        if (other.transform.IsChildOf(transform.parent)) return;

        GameObject target = other.GetComponentInParent<MonoBehaviour>()?.gameObject;
        if (target == null) return;

        if (target.GetComponentInChildren<FireEffect>() != null) return;

        GameObject newEffect = Instantiate(gameObject, target.transform);
        newEffect.transform.localPosition = Vector3.zero;
        newEffect.GetComponent<FireEffect>().spreading = false;
    }
}
 