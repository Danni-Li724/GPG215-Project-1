using UnityEngine;

public class PickUpItem : MonoBehaviour, IPickUp
{
    [SerializeField] private PickupType type = PickupType.Life;
    public PickupType Type => type;

    public void Consume()
    {
        gameObject.SetActive(false); // im gonna pool later
    }
}