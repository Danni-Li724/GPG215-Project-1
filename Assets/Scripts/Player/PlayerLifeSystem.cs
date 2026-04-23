using System.Collections;
using UnityEngine;

public class PlayerLifeSystem : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerCollectionSystem lifeInventory;
    [SerializeField] private LifeUIAnimation lifeUI;
    [SerializeField] private GameObject playerLifeObject;

    [Header("SFX")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip breakClip;
    [SerializeField] private AudioClip reviveClip;

    [Header("I-frames")]
    [SerializeField] private float iFrameSeconds = 0.5f;

    [Header("Ad Prompt")]
    [SerializeField] private AdPromptPanelUI adPromptPanel;

    private bool isProcessingHit;
    private float iFrameTimer;
    private int livesLost;
    private bool adReviveUsed;

    public int LivesLost => livesLost;
    public System.Action OnHitReceived;
    
    [SerializeField] private ShieldHealth shield;

    private void Awake()
    {
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.spatialBlend = 0f;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isProcessingHit || iFrameTimer > 0f) return;
        IDanger danger = other.GetComponentInParent<IDanger>();
        if (danger == null) return;
        if (shield != null && shield.gameObject.activeSelf) return;
        OnHitReceived?.Invoke();
        StartCoroutine(HandleLifeLossRoutine());
    }
    private IEnumerator HandleLifeLossRoutine()
    {
        isProcessingHit = true;
        livesLost++;

        if (playerLifeObject != null) playerLifeObject.SetActive(false);
        if (breakClip != null)
        {
            sfxSource.PlayOneShot(breakClip);
            yield return new WaitForSecondsRealtime(Mathf.Min(breakClip.length, 1.5f));
        }
        int storedLives = lifeInventory != null ? lifeInventory.CurrentLives : 0;

        if (storedLives > 0)
        {
            if (lifeUI != null) lifeUI.HideLifeIconByCount(storedLives);
            if (lifeInventory != null) lifeInventory.ForceSetLives(storedLives - 1);

            if (reviveClip != null)
            {
                sfxSource.PlayOneShot(reviveClip);
                yield return new WaitForSecondsRealtime(Mathf.Min(reviveClip.length, 1.5f));
            }

            if (playerLifeObject != null) playerLifeObject.SetActive(true);
            iFrameTimer = iFrameSeconds;
            isProcessingHit = false;
            yield break;
        }
        
        isProcessingHit = false;

        if (!adReviveUsed && adPromptPanel != null)
        {
            adReviveUsed = true;
            GameManager.instance.isRunning = false;

            adPromptPanel.Show(
                onWatchAd: () =>
                {
                    if (playerLifeObject != null) playerLifeObject.SetActive(true);
                    iFrameTimer = iFrameSeconds;
                    GameManager.instance.ResumeRun();
                },
                onDecline: () =>
                {
                    if (GameManager.instance != null)
                        GameManager.instance.GameOver(livesLost);
                }
            );
            yield break;
        }
        if (GameManager.instance != null)
            GameManager.instance.GameOver(livesLost);
    }

    private void Update()
    {
        if (iFrameTimer > 0f) iFrameTimer -= Time.deltaTime;
    }
}
