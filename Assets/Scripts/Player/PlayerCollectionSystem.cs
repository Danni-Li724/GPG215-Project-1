using UnityEngine;

public class PlayerCollectionSystem : MonoBehaviour
{
    [Header("Lives")]
    [SerializeField] private int maxLives = 5;

    [Header("UI")]
    [SerializeField] private LifeUIAnimation lifeUI;

    private int currentLives;

    public int CurrentLives => currentLives;

    private void OnTriggerEnter2D(Collider2D other)
    {
        IPickUp pickUp = other.GetComponentInParent<IPickUp>();
        if (pickUp == null)
            return;

        if (pickUp.Type == PickupType.Life)
        {
            TryCollectLife(pickUp);
        }
        else
        {
            // powerups to implement later
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