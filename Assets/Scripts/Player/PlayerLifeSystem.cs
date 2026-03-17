using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

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

    private bool isProcessingHit;
    private float iFrameTimer;

    private int livesLost;

    public int LivesLost => livesLost;

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
        if (isProcessingHit)
            return;

        if (iFrameTimer > 0f)
            return;

        IDanger danger = other.GetComponentInParent<IDanger>();
        if (danger == null)
            return;

        StartCoroutine(HandleLifeLossRoutine());
    }

    private IEnumerator HandleLifeLossRoutine()
    {
        isProcessingHit = true;
        livesLost += 1;

        // player life 'shatters'
        if (playerLifeObject != null)
            playerLifeObject.SetActive(false);

        if (breakClip != null)
            sfxSource.PlayOneShot(breakClip);

        yield return WaitClipSeconds(breakClip);

        int storedLives = (lifeInventory != null) ? lifeInventory.CurrentLives : 0;

        if (storedLives > 0)
        {
            // spend one stored life 
            if (lifeUI != null)
                lifeUI.HideLifeIconByCount(storedLives);

            if (lifeInventory != null)
                lifeInventory.ForceSetLives(storedLives - 1);

            // play revive clip and restore life
            if (reviveClip != null)
                sfxSource.PlayOneShot(reviveClip);

            yield return WaitClipSeconds(reviveClip);

            if (playerLifeObject != null)
                playerLifeObject.SetActive(true);

            iFrameTimer = iFrameSeconds;
        }
        else
        {
            // game over
           if (GameManager.instance != null)
                GameManager.instance.GameOver(livesLost);
        }

        isProcessingHit = false;
    }

    private IEnumerator WaitClipSeconds(AudioClip clip)
    {
        if (clip == null)
            yield break;

        yield return new WaitForSecondsRealtime(clip.length);
    }

    private void Update()
    {
        if (iFrameTimer > 0f)
            iFrameTimer -= Time.deltaTime;
    }
}
