using TMPro;
using UnityEngine;

public class ProceedToNextLevelPanelUI : MonoBehaviour
{
    [SerializeField] private TMP_Text enemiesKilledText;
    [SerializeField] private TMP_Text mileageTraveledText;
    [SerializeField] private TMP_Text mileageRemainingText;
    [SerializeField] private TMP_Text livesLostText;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Show(int enemiesKilled, int traveled, int remaining, int livesLost)
    {
        if (enemiesKilledText != null) enemiesKilledText.text = "Enemies Killed: " + enemiesKilled.ToString();
        if (mileageTraveledText != null) mileageTraveledText.text = "Mileage Traveled: " + traveled.ToString();
        if (mileageRemainingText != null) mileageRemainingText.text = "Mileage Remaining: " + remaining.ToString();
        if (livesLostText != null) livesLostText.text = "Lives Lost: " + livesLost.ToString();

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}