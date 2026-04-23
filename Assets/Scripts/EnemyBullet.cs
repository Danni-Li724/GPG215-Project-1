using UnityEngine;

public class EnemyBullet : MonoBehaviour, IDanger
{
    private float lifeRemaining;
    private Vector2 velocity;
    private bool isActive;

    public bool IsActive => isActive;

    public void Activate(Vector2 position, Vector2 direction, float speed, float lifetimeSeconds)
    {
        transform.position = position;

        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector2.down;
        direction.Normalize();

        velocity = direction * speed;
        lifeRemaining = lifetimeSeconds;

        isActive = true;
        gameObject.SetActive(true);
    }

    public void Tick(float dt)
    {
        if (!isActive)
            return;

        Vector3 p = transform.position;
        p.x += velocity.x * dt;
        p.y += velocity.y * dt;
        transform.position = p;

        lifeRemaining -= dt;
        if (lifeRemaining <= 0f)
            Deactivate();
    }

    public void Deactivate()
    {
        isActive = false;
        gameObject.SetActive(false);
    }
}
