using UnityEngine;

public class PlayerCollectionSystem : MonoBehaviour
{
    [Header("Lives")]
    [SerializeField] private int maxLives = 5;

    [Header("UI")]
    [SerializeField] private LifeUIAnimation lifeUI;
    
    [SerializeField] private PlayerPowerUpSystem powerUpSystem;

    private int currentLives;

    public int CurrentLives => currentLives;

    private void OnTriggerEnter2D(Collider2D other)
    {
        IPickUp pickUp = other.GetComponentInParent<IPickUp>();
        if (pickUp == null) return;

        if (pickUp.Type == PickupType.Life)
            TryCollectLife(pickUp);
        else if (pickUp.Type == PickupType.PowerUp)
        {
            PowerUpPickupItem powerUp = other.GetComponentInParent<PowerUpPickupItem>();
            if (powerUp != null && powerUpSystem != null)
            {
                powerUpSystem.Activate(powerUp.PowerUpType);
                pickUp.Consume();
            }
        }
    }

    private void TryCollectLife(IPickUp pickUp)
    {
        if (currentLives >= maxLives)
            return;

        currentLives += 1;
        pickUp.Consume();

        if (lifeUI != null)
            lifeUI.PlayLifeCollectAnimationAndThenReveal(currentLives);
    }
    
    public void ForceSetLives(int newValue)
    {
        currentLives = Mathf.Clamp(newValue, 0, maxLives);
    }
}