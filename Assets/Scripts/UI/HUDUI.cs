using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;

public class HUDUI : MonoBehaviour
{
    [Header("Options Panel")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private RectTransform optionsPanelGraphic;
    [SerializeField] private RectTransform optionsPanelEnd;
    [SerializeField] private float optionsPanelHideSeconds = 0.25f;
    private Coroutine optionsPanelRoutine;
    [SerializeField] private float optionsPanelMoveDuration = 0.5f;
    [SerializeField] private Ease optionsPanelEase = Ease.OutCubic;
    private Vector2 optionsPanelStartPos;
    private bool optionsOpen;

    [SerializeField] private GameObject pausePanel;

    [Header("Boss Notice")]
    [SerializeField] private TMP_Text bossNoticeText;
    [SerializeField] private RectTransform bossNoticeStart;
    [SerializeField] private RectTransform bossNoticeOnScreen;
    [SerializeField] private RectTransform bossNoticeExit;
    [SerializeField] private float bossNoticeSwipeSeconds = 0.35f;

    [Header("Arrival Notice")]
    [SerializeField] private TMP_Text arrivalNoticeText;
    [SerializeField] private RectTransform arrivalNoticeStart;
    [SerializeField] private RectTransform arrivalNoticeOnScreen;
    [SerializeField] private RectTransform arrivalNoticeExit;
    [SerializeField] private float arrivalNoticeSwipeSeconds = 0.35f;

    
    [Header("End Panels")]
    [SerializeField] private GameOverPanelUI gameOverPanel;
    [SerializeField] private ProceedToNextLevelPanelUI proceedToNextLevelPanel;
    
    [Header("High Score")]
    [SerializeField] private TMP_Text highScoreNoticeText;
    [SerializeField] private float highScoreNoticeSeconds = 2.5f;
    private Coroutine highScoreRoutine;

    private void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (optionsPanelGraphic != null) optionsPanelStartPos = optionsPanelGraphic.anchoredPosition;
        if (highScoreNoticeText != null) highScoreNoticeText.gameObject.SetActive(false);
    }

    // allow GameManager to bind panels 
    public void BindPanels(GameOverPanelUI gameOver)
    {
        if (gameOverPanel == null)
            gameOverPanel = gameOver;
    }

    public void OnStopPlaying()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (GameManager.instance != null)
        {
            int livesLost = GameManager.instance.playerLife != null
                ? GameManager.instance.playerLife.LivesLost
                : 0;
            GameManager.instance.GameOver(livesLost);
        }
    }
    
    public void OnBackToMenuPressed()
    {
        SceneManager.LoadScene("Menu");
    }

    public void OnBackToGamePressed()
    {
        pausePanel.SetActive(false);
        GameManager.instance.isRunning = true;
    }

    public void OnPausePressed()
    {
        pausePanel.SetActive(true);
        GameManager.instance.isRunning = false;
    }

    public void OnOptionsPressed()
    {
        optionsOpen = !optionsOpen;
        if (optionsOpen)
        {
            optionsPanel.SetActive(true);
            optionsPanelGraphic.anchoredPosition = optionsPanelStartPos;
        }

        optionsPanelGraphic.DOKill();
        Vector2 target = optionsOpen ? optionsPanelEnd.anchoredPosition : optionsPanelStartPos;
        optionsPanelGraphic.DOAnchorPos(target, optionsPanelMoveDuration)
            .SetEase(optionsPanelEase)
            .OnComplete(() =>
            {
                if (!optionsOpen) optionsPanel.SetActive(false);
            });
    }

    public void HideOptionsPanel()
    {
        if (optionsPanelRoutine != null)
            StopCoroutine(optionsPanelRoutine);
        optionsPanelRoutine = StartCoroutine(HideOptionsPanelRoutine());
    }

    private IEnumerator HideOptionsPanelRoutine()
    {
        if (!optionsPanelGraphic.gameObject.activeSelf)
            yield break;

        Vector2 from = optionsPanelGraphic.anchoredPosition;
        Vector2 to = optionsPanelStartPos;

        float duration = Mathf.Max(0.01f, optionsPanelHideSeconds);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duration;
            float u = Mathf.Clamp01(t);
            optionsPanelGraphic.anchoredPosition = Vector2.Lerp(from, to, u);
            yield return null;
        }

        optionsPanelGraphic.anchoredPosition = to;
        optionsPanelRoutine = null;

        GameManager.instance.isRunning = true;
        ResetOptions();
    }

    public void ResetOptions()
    {
        optionsPanel.SetActive(false);
        optionsPanelGraphic.anchoredPosition = optionsPanelStartPos;
        optionsOpen = false;
        // if (GameManager.instance != null) GameManager.instance.isRunning = true;
    }

    public void PlayBossNotice(string message, float holdSeconds, System.Action onComplete)
    {
        PlayTextSwipeNotice(
            bossNoticeText,
            bossNoticeStart,
            bossNoticeOnScreen,
            bossNoticeExit,
            bossNoticeSwipeSeconds,
            holdSeconds,
            message,
            onComplete
        );
    }

    public void PlayArrivalNotice(string message, float holdSeconds, System.Action onComplete)
    {
        PlayTextSwipeNotice(
            arrivalNoticeText,
            arrivalNoticeStart,
            arrivalNoticeOnScreen,
            arrivalNoticeExit,
            arrivalNoticeSwipeSeconds,
            holdSeconds,
            message,
            onComplete
        );
    }

    private void PlayTextSwipeNotice(
        TMP_Text text,
        RectTransform start,
        RectTransform onScreen,
        RectTransform exit,
        float swipeSeconds,
        float holdSeconds,
        string message,
        Action onComplete)
    {
        if (text == null || start == null || onScreen == null || exit == null)
        {
            onComplete?.Invoke();
            return;
        }

        RectTransform rt = text.rectTransform;

        text.text = message;
        text.gameObject.SetActive(true);

        rt.DOKill();
        rt.anchoredPosition = start.anchoredPosition;

        float hold = Mathf.Max(0f, holdSeconds);

        Sequence seq = DOTween.Sequence();
        seq.Append(rt.DOAnchorPos(onScreen.anchoredPosition, swipeSeconds).SetEase(Ease.OutCubic));
        seq.AppendInterval(hold);
        seq.Append(rt.DOAnchorPos(exit.anchoredPosition, swipeSeconds).SetEase(Ease.InCubic));
        seq.OnComplete(() =>
        {
            text.gameObject.SetActive(false);
            onComplete?.Invoke();
        });
    }
    public void ShowGameOverPanel(GameManager.RunStats s)
    {
        if (proceedToNextLevelPanel != null)
            proceedToNextLevelPanel.Hide();

        if (gameOverPanel != null)
            gameOverPanel.ShowGameOver(s.enemiesKilled, s.mileageTraveled, s.mileageRemaining, s.livesLost);
    }

    public void ShowProceedToNextLevelPanel(GameManager.RunStats state)
    {
        Debug.Log("Show proceed to next level panel");
        if (gameOverPanel != null)
            gameOverPanel.Hide();
        if (proceedToNextLevelPanel != null)
            proceedToNextLevelPanel.Show(state.enemiesKilled, state.mileageTraveled, state.mileageRemaining, state.livesLost);
    }
    
    // public void ShowHighScoreNotice()
    // {
    //     if (highScoreNoticeText == null) return;
    //     if (highScoreRoutine != null) StopCoroutine(highScoreRoutine);
    //     highScoreRoutine = StartCoroutine(HighScoreNoticeRoutine());
    // }
    //
    // private IEnumerator HighScoreNoticeRoutine()
    // {
    //     highScoreNoticeText.gameObject.SetActive(true);
    //     yield return new WaitForSeconds(highScoreNoticeSeconds);
    //     highScoreNoticeText.gameObject.SetActive(false);
    //     highScoreRoutine = null;
    // }
    
  // now passes actual score to notifier
    public void ShowHighScoreNotice(int mileage)
    {
        if (highScoreNoticeText == null) return;
        HighScoreNotifier notifier = highScoreNoticeText.GetComponent<HighScoreNotifier>();
        if (notifier != null) notifier.Show(mileage);
    }


    public void OnTryAgainPressed()
    {
        // reload scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        // GameManager.instance.RestartCurrentLevel();
    }

    public void OnNextLevelPressed()
    {
        GameManager.instance.StartNextLevel();
        proceedToNextLevelPanel?.Hide();
    }
    
}