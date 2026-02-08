using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;

    public void Move(Vector2 directionNormalized)
    {
        Vector3 delta = new Vector3(directionNormalized.x, directionNormalized.y, 0f);
        transform.position += delta * moveSpeed * Time.deltaTime;
    }
}
