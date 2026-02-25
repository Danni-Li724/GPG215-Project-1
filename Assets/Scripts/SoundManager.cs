using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip backgroundMusic;
    [Range(0f, 1f)][SerializeField] private float musicVolume = 0.6f;
    private void Awake()
    {
        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = musicVolume;
    }

    private void Start()
    {
        if (backgroundMusic == null)
            return;
        musicSource.clip = backgroundMusic;
        musicSource.Play();
    }
}