using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelUI : MonoBehaviour
{
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Toggle proceduralBgToggle;
    [SerializeField] private Button applyButton;
    [SerializeField] private Button closeButton;
    
    [SerializeField] private StarfieldController starfield;
    [SerializeField] private ProceduralMapGenerator mapGenerator;

    private void Awake()
    {
        applyButton.onClick.AddListener(OnApply);
        if (closeButton != null) closeButton.onClick.AddListener(OnClose);
        gameObject.SetActive(false);
    }

    public void Show()
    {
        SettingsData current = MirageSaveSystem.Instance != null
            ? MirageSaveSystem.Instance.LoadSettingsOrDefault()
            : new SettingsData();

        musicVolumeSlider.value    = current.musicVolume;
        sfxVolumeSlider.value      = current.sfxVolume;
        proceduralBgToggle.isOn    = current.proceduralBgEnabled;
        gameObject.SetActive(true);
    }

    private void OnApply()
    {
        SettingsData data = new SettingsData
        {
            musicVolume        = musicVolumeSlider.value,
            sfxVolume          = sfxVolumeSlider.value,
            proceduralBgEnabled = proceduralBgToggle.isOn
        };

        if (MirageSaveSystem.Instance != null)
            MirageSaveSystem.Instance.SaveSettings(data);
        if (SoundManager.instance != null)
            SoundManager.instance.ApplySettings(data);

        if (starfield != null)
            starfield.ApplySettings(data);

        if (mapGenerator != null)
            mapGenerator.SetEnabled(data.proceduralBgEnabled);
    }

    private void OnClose()
    {
        gameObject.SetActive(false);

        // if used in-game, resume
        if (GameManager.instance != null && GameManager.instance.isRunning == false)
            GameManager.instance.ResumeRun();
    }
}
