using UnityEngine;

public class Orbit : MonoBehaviour
{
    private Transform center;
    [SerializeField] private float radius = 2f;
    [SerializeField] private float degreesPerSecond = 90f;

    private float angle;

    private void Start()
    {
        center = FindObjectOfType<PlayerMovement>().transform;
    }

    private void DoOrbit()
    {
        if (center == null)
            return;
        angle += degreesPerSecond * Time.deltaTime;
        float rad = angle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * radius;
        transform.position = center.position + offset;
    }
}