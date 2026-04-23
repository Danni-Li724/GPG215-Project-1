using UnityEngine;

public enum PowerUpType { TriShot, RapidFire, Shield, Fireball }

public class PowerUpPickupItem : MonoBehaviour, IPickUp
{
    [SerializeField] private PowerUpType powerUpType;
    public PowerUpType PowerUpType => powerUpType;
    
    public PickupType Type => PickupType.PowerUp;
    public void Consume() => gameObject.SetActive(false);
}
