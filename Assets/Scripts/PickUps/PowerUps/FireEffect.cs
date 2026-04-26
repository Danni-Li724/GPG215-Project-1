using UnityEngine;
public class FireEffect : MonoBehaviour, ITickable
{
    [SerializeField] private float duration = 5f;
    [SerializeField] private float dps      = 1f;

    private IDamageable host;
    private Transform followTarget; 
    private float damageAccumulator;
    private float lifeRemaining;
    private bool spreading = true;
    private GameObject parentGO;
    private bool active;

    private void Start()
    {
        host          = GetComponentInParent<IDamageable>();
        lifeRemaining = duration;
        GetComponent<ParticleSystem>().Play();
        // cache parent reference on start
        parentGO = transform.parent?.gameObject;
        if (GameManager.instance != null)
            GameManager.instance.RegisterTickable(this);
    }
    
    public void Activate(Transform target)
    {
        // transform.SetParent(parent, false);
        // transform.localPosition = Vector3.zero;
        // host = parent.GetComponent<IDamageable>();
        
        transform.SetParent(null); // not set parent now because the reappearing is driving me insane
        followTarget = target;
        host = target.GetComponent<IDamageable>();
        
        lifeRemaining = duration;
        damageAccumulator = 0f;
        spreading = true;
        active = true;
        gameObject.SetActive(true);
        GetComponent<ParticleSystem>().Play();

        if (GameManager.instance != null)
            GameManager.instance.RegisterTickable(this);
    }

    // private void OnTransformParentChanged() // unity's native function automatically called when deactivating an object
    // {
    //     // only react if it's active and parent just went away/inactive
    //     if (!active) return;
    //     if (transform.parent == null) return;
    //     if (!transform.parent.gameObject.activeInHierarchy)
    //         DetachAndDeactivate();
    // }
    //
    // private void DetachAndDeactivate() // because UNity doesn't let me destroy object when activating/deactivating :( which is what my pool does
    // {
    //     active = false;
    //     // unparent FIRST so it's no longer a child when SetActive(false) runs
    //     transform.SetParent(null, true);
    //     GetComponent<ParticleSystem>().Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    //     gameObject.SetActive(false);
    //
    //     if (GameManager.instance != null)
    //         GameManager.instance.UnregisterTickable(this);
    // }
    
    public void Tick(float dt)
    {
        if (!active) return;

        // follow target position each tick. if target is gone or deactivated, expire immediately
        if (followTarget == null || !followTarget.gameObject.activeInHierarchy)
        {
            Expire();
            return;
        }
        transform.position = followTarget.position;

        lifeRemaining -= dt;
        damageAccumulator += dps * dt;
        if (damageAccumulator >= 1f)
        {
            int dmg = Mathf.FloorToInt(damageAccumulator);
            damageAccumulator -= dmg;
            host?.TakeDamage(dmg);
        }

        if (lifeRemaining <= 0f) Expire();
    }
    
    private void Expire()
    {
        active       = false;
        followTarget = null;
        GetComponent<ParticleSystem>().Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        Destroy(gameObject);

        if (GameManager.instance != null)
            GameManager.instance.UnregisterTickable(this);
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!active || !spreading) return;

        IPoolableEnemy enemy = other.GetComponentInParent<IPoolableEnemy>();
        if (enemy == null) return;

        GameObject target = other.GetComponentInParent<MonoBehaviour>()?.gameObject;
        if (target == null) return;

        // // don't spread to its own host
        // if (target == transform.parent?.gameObject) return;
        // // don't attach if already burning
        // if (target.GetComponentInChildren<FireEffect>() != null) return;
        
        FireEffect[] allEffects = FindObjectsByType<FireEffect>(FindObjectsSortMode.None);
        foreach (var e in allEffects)
            if (e.followTarget != null && e.followTarget.gameObject == target) return;
        
        FireEffect newEffect = Instantiate(this, target.transform);
        newEffect.spreading = false;
        newEffect.Activate(target.transform);
    }
    
    private void OnDisable()
    {
        if (GameManager.instance != null)
            GameManager.instance.UnregisterTickable(this);
    }
    
}
 