using UnityEngine;

public class SwayRotate : MonoBehaviour
{
    [SerializeField] private float maxDegrees = 15f;
    [SerializeField] private float frequency = 2f;

    private void DoSway()
    {
        float z = Mathf.Sin(Time.time * frequency) * maxDegrees;
        transform.rotation = Quaternion.Euler(0f, 0f, z);
    }
}