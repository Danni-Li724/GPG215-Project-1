using UnityEngine;

public class Bob : MonoBehaviour
{
    [SerializeField] private float amplitude = 0.25f;
    [SerializeField] private float frequency = 2f;
    private float bobTimer;

    public void ResetBob()
    {
        bobTimer = Random.Range(0f, Mathf.PI * 2f); 
    }

    public void DoBob(float dt)
    {
        bobTimer += dt * frequency;
        float yOffset = Mathf.Sin(bobTimer) * amplitude;
        Vector3 pos = transform.position;
        pos.y += yOffset * dt; 
        transform.position = pos;
    }
}