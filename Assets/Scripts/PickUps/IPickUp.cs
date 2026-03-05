using UnityEngine;

public interface IPickUp
{
    PickupType Type { get; }
    void Consume(); 
}

public enum PickupType
{
    PowerUp = 0,
    Life = 1
}
