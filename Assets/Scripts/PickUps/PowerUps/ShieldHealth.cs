using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
public class ShieldHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private Image healthBarFill;

    private int currentHealth;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Activate()
    {
        currentHealth = maxHealth;
        gameObject.SetActive(true);
        UpdateBar();
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    public void TakeDamage(int amount)
    {
        if (!gameObject.activeSelf) return;
        currentHealth = Mathf.Max(0, currentHealth - amount);
        UpdateBar();
        if (currentHealth <= 0) Deactivate();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!gameObject.activeSelf) return;

        IDanger danger = other.GetComponentInParent<IDanger>();
        if (danger == null) return;
        EnemyBullet enemyBullet = other.GetComponentInParent<EnemyBullet>();
        if (enemyBullet != null)
        {
            enemyBullet.Deactivate();
            TakeDamage(1);
            return;
        }

        IDamageable enemy = other.GetComponentInParent<IDamageable>();
        if (enemy != null && (object)enemy != this)
        {
            enemy.TakeDamage(9999);
            TakeDamage(1);
            return;
        }
    }
    private void UpdateBar()
    {
        if (healthBarFill == null) return;
        healthBarFill.fillAmount = (float)currentHealth / maxHealth;
    }
}