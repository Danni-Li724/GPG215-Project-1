using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelUI : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [Header("Toggles")]
    [SerializeField] private Toggle proceduralBgToggle;
    [SerializeField] private Toggle leftHandModeToggle;
    [Header("Buttons")]
    [SerializeField] private Button applyButton;
    [SerializeField] private Button closeButton;
    [Header("Refs")]
    [SerializeField] private StarfieldController starfield;
    [SerializeField] private ProceduralMapLocal mapGenerator;
    [SerializeField] private ControlAreaFlipper controlAreaFlipper;

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
        gameObject.SetActive(true);
        musicVolumeSlider.interactable = false;
        sfxVolumeSlider.interactable   = false;

        musicVolumeSlider.value     = current.musicVolume;
        sfxVolumeSlider.value       = current.sfxVolume;
        proceduralBgToggle.isOn     = current.proceduralBgEnabled;
        musicVolumeSlider.interactable = true;
        sfxVolumeSlider.interactable   = true;
    }

    private void OnApply()
    {
        SettingsData data = new SettingsData
        {
            musicVolume        = musicVolumeSlider.value,
            sfxVolume          = sfxVolumeSlider.value,
            proceduralBgEnabled = proceduralBgToggle.isOn,
            leftHandMode        = leftHandModeToggle.isOn
        };

        if (MirageSaveSystem.Instance != null)
            MirageSaveSystem.Instance.SaveSettings(data);
        if (SoundManager.instance != null)
            SoundManager.instance.ApplySettings(data);

        if (starfield != null)
            starfield.ApplySettings(data);

        if (mapGenerator != null)
            mapGenerator.SetEnabled(data.proceduralBgEnabled);
        
        if (controlAreaFlipper != null)
            controlAreaFlipper.ApplyHandMode(data.leftHandMode);
    }

    private void OnClose()
    {
        gameObject.SetActive(false);

        // if used in-game, resume
        if (GameManager.instance != null && GameManager.instance.isRunning == false)
            GameManager.instance.ResumeRun();
    }
}
