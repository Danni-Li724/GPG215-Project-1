using UnityEngine;

public class Drift : MonoBehaviour
{
    [SerializeField] private Vector2 velocity = new Vector2(1f, 0f);

    private void DoDrift()
    {
        transform.position += (Vector3)(velocity * Time.deltaTime);
    }
}
