using UnityEngine;

public class Bob : MonoBehaviour
{
    [SerializeField] private float amplitude = 0.25f;
    [SerializeField] private float frequency = 2f;

    private Vector3 startPos;

    private void Awake()
    {
        startPos = transform.position;
    }

    public void DoBob()
    {
        float y = Mathf.Sin(Time.time * frequency) * amplitude;
        transform.position = startPos + new Vector3(0f, y, 0f);
    }
}
