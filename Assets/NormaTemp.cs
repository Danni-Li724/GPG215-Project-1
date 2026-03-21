using UnityEngine;

public class NormaTemp : MonoBehaviour
{
    public Rotate rotate;
    void Awake()
    {
        rotate = GetComponent<Rotate>();
    }
    void Update()
    {
        rotate.DoRotate();
    }
}
