using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class AdPromptPanelUI : MonoBehaviour
{
    [SerializeField] private Button watchAdButton;
    [SerializeField] private Button declineButton;
    [SerializeField] private TMP_Text countdownText;  
    [SerializeField] private float simulatedAdSeconds = 3f;

    private Action onWatchAd;
    private Action onDecline;
    private Coroutine adRoutine;

    private void Awake()
    {
        gameObject.SetActive(false);
        watchAdButton.onClick.AddListener(OnWatchAdPressed);
        declineButton.onClick.AddListener(OnDeclinePressed);
    }

    public void Show(Action onWatchAd, Action onDecline)
    {
        this.onWatchAd = onWatchAd;
        this.onDecline = onDecline;
        gameObject.SetActive(true);
        watchAdButton.interactable = true;
        declineButton.interactable = true;
        if (countdownText != null) countdownText.gameObject.SetActive(false);
    }

    private void OnWatchAdPressed()
    {
        watchAdButton.interactable = false;
        declineButton.interactable = false;
        if (adRoutine != null) StopCoroutine(adRoutine);
        adRoutine = StartCoroutine(SimulateAdRoutine());
    }

    private IEnumerator SimulateAdRoutine()
    {
        if (countdownText != null) countdownText.gameObject.SetActive(true);

        float remaining = simulatedAdSeconds;
        while (remaining > 0f)
        {
            if (countdownText != null)
                countdownText.text = $"Ad ends in {Mathf.CeilToInt(remaining)}s";
            remaining -= Time.unscaledDeltaTime;
            yield return null;
        }

        if (countdownText != null) countdownText.gameObject.SetActive(false);
        gameObject.SetActive(false);
        onWatchAd?.Invoke();
    }

    private void OnDeclinePressed()
    {
        if (adRoutine != null) StopCoroutine(adRoutine);
        gameObject.SetActive(false);
        onDecline?.Invoke();
    }
}
