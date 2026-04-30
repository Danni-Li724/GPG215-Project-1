using UnityEngine;

// Attach to any powerup prefab.
// Falls quickly on spawn, slows down at slowY giving player time to grab it,
// then crawls until destroyY where it is destroyed.
public class PowerUpDrifter : MonoBehaviour
{
    [Header("Y Thresholds (world space)")]
    [Tooltip("Y position where falling speed switches to slow drift")]
    [SerializeField] private float slowY     = -1f;

    [Tooltip("Y position where powerup is destroyed if not collected")]
    [SerializeField] private float destroyY  = -6f;

    [Header("Speeds")]
    [Tooltip("Fall speed before reaching slowY")]
    [SerializeField] private float fastSpeed = 2.5f;

    [Tooltip("Drift speed after reaching slowY")]
    [SerializeField] private float slowSpeed = 0.2f;

    private void Update()
    {
        float speed = transform.position.y > slowY ? fastSpeed : slowSpeed;
        transform.position += Vector3.down * speed * Time.deltaTime;

        if (transform.position.y < destroyY)
            Destroy(gameObject);
    }
}
