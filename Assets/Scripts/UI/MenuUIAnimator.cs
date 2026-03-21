using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;
using UnityEngine.Serialization;

public class MenuUIAnimator : MonoBehaviour
{
    [Header("Main Images")]
    [SerializeField] private RectTransform UpTerrainImage;
    [SerializeField] private RectTransform DownTerrainImage;
    [SerializeField] private RectTransform UpTerrainTarget;
    [SerializeField] private RectTransform DownTerrainTarget;

    [Header("Text")]
    [SerializeField] private RectTransform titleText;  
    [SerializeField] private RectTransform nameText;

    [Header("Buttons")]
    [SerializeField] private RectTransform startButton;
    [SerializeField] private RectTransform optionsButton;
    [SerializeField] private RectTransform startButtonTarget;
    [SerializeField] private RectTransform optionsButtonTarget;

    [Header("Options Panel")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private RectTransform optionsPanelGraphic;
    [SerializeField] private RectTransform optionsPanelEnd;
    [SerializeField] private float optionsPanelHideSeconds = 0.25f;
    private Coroutine optionsPanelRoutine;

    [Header("Timings")]
    [SerializeField] private float imagesMoveDuration = 0.8f;
    [SerializeField] private float textPopDuration = 0.35f;
    [SerializeField] private float buttonsMoveDuration = 0.55f;
    [SerializeField] private float optionsPanelMoveDuration = 0.5f;

    [Header("Ease")]
    [SerializeField] private Ease imagesMoveEase = Ease.OutCubic;
    [SerializeField] private Ease popEase = Ease.OutBack;
    [SerializeField] private Ease buttonsMoveEase = Ease.OutCubic;
    [SerializeField] private Ease optionsPanelEase = Ease.OutCubic;

    [Header("Idle Pop")]
    [SerializeField] private float idlePopScaleAmount = 0.03f;
    [SerializeField] private float idlePopDuration = 1.4f;

    private Sequence introSequence;

    private Vector2 image1StartPos;
    private Vector2 image2StartPos;

    private Vector2 startButtonStartPos;
    private Vector2 optionsButtonStartPos;

    private Vector2 optionsPanelStartPos;
    private bool optionsOpen;

    private Tween titleIdleTween;
    private Tween startIdleTween;
    private Tween optionsIdleTween;

    private void Awake()
    {
        image1StartPos = UpTerrainImage.anchoredPosition;
        image2StartPos = DownTerrainImage.anchoredPosition;
        startButtonStartPos = startButton.anchoredPosition;
        optionsButtonStartPos = optionsButton.anchoredPosition;
        optionsPanelStartPos = optionsPanelGraphic.anchoredPosition;
        titleText.localScale = Vector3.zero;
        nameText.localScale = Vector3.zero;
    }

    private void OnDisable()
    {
        KillAllTweens();
    }
    
    private void Start()
    {
        PlayIntro();
        SoundManager.instance.SetMenuState();
    }

    public void PlayIntro()
    {
        optionsPanel.SetActive(false);
        if (introSequence != null && introSequence.IsActive())
            return;

        KillAllTweens();
        ResetIntroState();

        introSequence = DOTween.Sequence();

        // move images to targets together
        introSequence.Append(UpTerrainImage.DOAnchorPos(UpTerrainTarget.anchoredPosition, imagesMoveDuration).SetEase(imagesMoveEase));
        introSequence.Join(DownTerrainImage.DOAnchorPos(DownTerrainTarget.anchoredPosition, imagesMoveDuration).SetEase(imagesMoveEase));

        // landing punch
        introSequence.Join(UpTerrainImage.DOPunchScale(new Vector3(0.03f, 0.03f, 0f), 0.25f, 8, 0.8f));
        introSequence.Join(DownTerrainImage.DOPunchScale(new Vector3(0.03f, 0.03f, 0f), 0.25f, 8, 0.8f));

        // title pops in
        introSequence.Append(titleText.DOScale(Vector3.one, textPopDuration).SetEase(popEase));
        introSequence.AppendCallback(() =>
        {
            StartIdlePop(titleText, ref titleIdleTween);
        });

        // // name pops in 
        introSequence.Append(nameText.DOScale(Vector3.one, textPopDuration).SetEase(popEase));

        // buttons slide up to targets, then pop
        introSequence.Append(startButton.DOAnchorPos(startButtonTarget.anchoredPosition, buttonsMoveDuration).SetEase(buttonsMoveEase));
        introSequence.Join(optionsButton.DOAnchorPos(optionsButtonTarget.anchoredPosition, buttonsMoveDuration).SetEase(buttonsMoveEase));

        introSequence.Join(startButton.DOPunchScale(new Vector3(0.06f, 0.06f, 0f), 0.3f, 10, 0.9f));
        introSequence.Join(optionsButton.DOPunchScale(new Vector3(0.06f, 0.06f, 0f), 0.3f, 10, 0.9f));

        introSequence.AppendCallback(() =>
        {
            StartIdlePop(startButton, ref startIdleTween);
            StartIdlePop(optionsButton, ref optionsIdleTween);
        });

        introSequence.SetUpdate(true);
    }

    public void OnStartPressed()
    {
        // TODO: play a quick outro then load?
        KillAllTweens();

        Sequence outro = DOTween.Sequence();
        outro.Append(transform.DOScale(1f, 0.01f)); 
        outro.AppendInterval(0.05f);
        outro.AppendCallback(() => SceneManager.LoadScene("Game"));
    }

    public void OnOptionsPressed()
    {
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
        ResetOptions();
    }

    private void StartIdlePop(RectTransform rt, ref Tween storeTween)
    {
        if (storeTween != null && storeTween.IsActive())
            storeTween.Kill();

        rt.localScale = Vector3.one;

        Vector3 upScale = Vector3.one * (1f + idlePopScaleAmount);

        storeTween = rt.DOScale(upScale, idlePopDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void ResetIntroState()
    {
        // positions
        UpTerrainImage.anchoredPosition = image1StartPos;
        DownTerrainImage.anchoredPosition = image2StartPos;

        startButton.anchoredPosition = startButtonStartPos;
        optionsButton.anchoredPosition = optionsButtonStartPos;

        // scales
        UpTerrainImage.localScale = Vector3.one;
        DownTerrainImage.localScale = Vector3.one;
        startButton.localScale = Vector3.one;
        optionsButton.localScale = Vector3.one;

        titleText.localScale = Vector3.zero;
        nameText.localScale = Vector3.zero;

        ResetOptions();
    }

    public void ResetOptions()
    {
        optionsPanel.SetActive(false);
        optionsPanelGraphic.anchoredPosition = optionsPanelStartPos;
        optionsOpen = false;
    }

    private void KillAllTweens()
    {
        introSequence?.Kill();
        introSequence = null;

        titleIdleTween?.Kill();
        startIdleTween?.Kill();
        optionsIdleTween?.Kill();

        UpTerrainImage.DOKill();
        DownTerrainImage.DOKill();
        titleText.DOKill();
        nameText.DOKill();
        startButton.DOKill();
        optionsButton.DOKill();
        optionsPanelGraphic.DOKill();
    }
}