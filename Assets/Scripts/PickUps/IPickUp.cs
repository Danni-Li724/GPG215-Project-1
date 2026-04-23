using UnityEngine;

public interface IPickUp
{
    PickupType Type { get; }
    void Consume();
}
