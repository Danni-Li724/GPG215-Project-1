using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverPanelUI : MonoBehaviour
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
        if (enemiesKilledText != null)
            enemiesKilledText.text = enemiesKilled.ToString();

        if (mileageTraveledText != null)
            mileageTraveledText.text = traveled.ToString();

        if (mileageRemainingText != null)
            mileageRemainingText.text = remaining.ToString();

        if (livesLostText != null)
            livesLostText.text = livesLost.ToString();

        gameObject.SetActive(true);
    }

    public void OnQuitClicked()
    {
        SceneManager.LoadScene("Menu");
    }
}
