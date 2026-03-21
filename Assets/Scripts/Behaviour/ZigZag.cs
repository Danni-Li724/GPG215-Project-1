using UnityEngine;

public class ZigZag : MonoBehaviour
{
    [SerializeField] private Vector2 forward = Vector2.right;
    [SerializeField] private float forwardSpeed = 2f;
    [SerializeField] private float zigAmplitude = 0.5f;
    [SerializeField] private float zigFrequency = 3f;

    private Vector3 startPos;

    private void Awake()
    {
        startPos = transform.position;
        forward = forward.sqrMagnitude < 0.001f ? Vector2.right : forward.normalized;
    }

    public void DoZigZag()
    {
        float t = Time.time;
        Vector2 perp = new Vector2(-forward.y, forward.x);

        Vector2 move = forward * forwardSpeed * Time.deltaTime;
        Vector2 offset = perp * (Mathf.Sin(t * zigFrequency) * zigAmplitude);

        startPos += (Vector3)move;
        transform.position = startPos + (Vector3)offset;
    }
}