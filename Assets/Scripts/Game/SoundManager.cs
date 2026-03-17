using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip menuMusic;
    [Range(0f, 1f)][SerializeField] private float musicVolume = 0.6f;
    public static SoundManager instance;

    public bool inMenu;
    public bool inGame;
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();
    }

    private void PlaySequence()
    {
        if (inMenu)
        {
            PlayMusic(menuMusic);
            return;
        }
        if (inGame)
            PlayMusic(backgroundMusic);
    }
    
    public void SetMenuState()
    {
        inMenu = true;
        inGame = false;
        PlaySequence();
    }

    public void SetGameState()
    {
        inMenu = false;
        inGame = true;
        PlaySequence();
    }

    private void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        musicSource.clip = clip;
        musicSource.Play();
    }
}