using UnityEngine;

public class LevelBoss : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 30;
    private int health;
    private bool isAlive;

    public void Activate(Vector2 position)
    {
        transform.position = position;
        health = maxHealth;
        isAlive = true;
        gameObject.SetActive(true);
    }

    public void TakeDamage(int amount)
    {
        if (!isAlive)
            return;

        health -= amount;

        if (health <= 0)
            Die();
    }

    private void Die()
    {
        isAlive = false;
        gameObject.SetActive(false);

        if (GameManager.instance != null)
            GameManager.instance.OnBossKilled();
    }
}