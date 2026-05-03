using UnityEngine;
public class PowerUpDrifter : MonoBehaviour
{
    [Header("Y Bounds")]
    [SerializeField] private float slowY     = -1f;
    [SerializeField] private float destroyY  = -6f;

    [Header("Speeds")]
    [SerializeField] private float fastSpeed = 2.5f;
    [SerializeField] private float slowSpeed = 0.2f;

    private void Update()
    {
        float speed = transform.position.y > slowY ? fastSpeed : slowSpeed;
        transform.position += Vector3.down * speed * Time.deltaTime;

        if (transform.position.y < destroyY)
            Destroy(gameObject);
    }
}
