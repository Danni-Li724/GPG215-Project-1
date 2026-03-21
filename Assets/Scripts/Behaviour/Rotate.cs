using UnityEngine;

public class Rotate : MonoBehaviour
{
    [SerializeField] private float degreesPerSecond = 90f;

    public void DoRotate()
    {
        transform.Rotate(0f, 0f, degreesPerSecond * Time.deltaTime);
    }
}