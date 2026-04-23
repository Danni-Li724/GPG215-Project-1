using UnityEngine;

public enum PickupType
{
    PowerUp = 0,
    Life    = 1
}

public class PickUpItem : MonoBehaviour, IPickUp
{
    [SerializeField] private PickupType type = PickupType.Life;
    public PickupType Type => type;
    public void Consume() => gameObject.SetActive(false);
}