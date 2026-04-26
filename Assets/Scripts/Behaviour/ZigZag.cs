using UnityEngine;

public class ZigZag : MonoBehaviour
{
    [SerializeField] private float zigAmplitude   = 1.5f;
    [SerializeField] private float zigFrequency   = 2f;
    [SerializeField] private float boundaryX      = 2.5f;
    private float zigTimer;
    private float horizontalDirection = 1f;

    public void ResetZigZag()
    {
        zigTimer           = Random.Range(0f, Mathf.PI * 2f);
        horizontalDirection = Random.value > 0.5f ? 1f : -1f;
    }

    public void DoZigZag(float dt)
    {
        zigTimer += dt * zigFrequency;
        float xOffset = Mathf.Sin(zigTimer) * zigAmplitude * horizontalDirection * dt;

        Vector3 pos = transform.position;
        pos.x += xOffset;
        if (pos.x > boundaryX || pos.x < -boundaryX)
        {
            horizontalDirection *= -1f;
            pos.x = Mathf.Clamp(pos.x, -boundaryX, boundaryX);
        }

        transform.position = pos;
    }
}