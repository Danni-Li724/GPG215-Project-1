using UnityEngine;

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private BoxCollider2D confiner;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    public void Move(Vector2 directionNormalized)
    {
        moveInput = Vector2.ClampMagnitude(directionNormalized, 1f);
    }
    private void FixedUpdate()
    {
        Vector2 current = rb.position;
        Vector2 next = current + (moveInput * moveSpeed * Time.fixedDeltaTime);
        // clamp within area
        if (confiner != null && !confiner.OverlapPoint(next))
            next = confiner.ClosestPoint(next);
        rb.MovePosition(next);
    }
}

