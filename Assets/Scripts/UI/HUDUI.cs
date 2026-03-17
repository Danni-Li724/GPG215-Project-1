using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

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

    void Awake()
    {
        pausePanel.SetActive(false);
        optionsPanelStartPos = optionsPanelGraphic.anchoredPosition;
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
        if(pausePanel) pausePanel.SetActive(false);
        GameManager.instance.isRunning = false;
        optionsPanel.SetActive(true);
        // pull-down panel graphic
        optionsOpen = !optionsOpen;
        optionsPanelGraphic.DOKill();

        Vector2 target = optionsOpen ? optionsPanelEnd.anchoredPosition : optionsPanelStartPos;
        optionsPanelGraphic.DOAnchorPos(target, optionsPanelMoveDuration).SetEase(optionsPanelEase);
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
    }

}
