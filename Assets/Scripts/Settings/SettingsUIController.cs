using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SettingsUIController : MonoBehaviour
{
    [Header("Graphics UI")]
    [SerializeField] private TMP_Dropdown windowModeDropdown;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown frameRateDropdown;
    [SerializeField] private Toggle vSyncToggle;
    [SerializeField] private Slider renderScaleSlider;
    [SerializeField] private TextMeshProUGUI renderScaleText;

    [Header("Audio UI")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private TextMeshProUGUI masterVolumeText;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private TextMeshProUGUI sfxVolumeText;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private TextMeshProUGUI musicVolumeText;

    [Header("Buttons")]
    [SerializeField] private Button applyButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button quitYesButton;
    [SerializeField] private Button quitNoButton;

    [Header("Other")]
    [SerializeField] private GameObject exitConfirmationPopup;

    private void Start()
    {
        InitializeUI();
        LoadCurrentSettings();
        SetupListeners();
    }

    private void InitializeUI()
    {
        // Window Mode Dropdown
        windowModeDropdown.ClearOptions();
        windowModeDropdown.AddOptions(new List<string>
        {
            "Fullscreen",
            "Borderless Window",
            "Windowed"
        });

        // Resolution Dropdown
        resolutionDropdown.ClearOptions();
        Resolution[] resolutions = SettingsManager.Instance.GetAvailableResolutions();
        List<string> resolutionStrings = new List<string>();

        foreach (Resolution res in resolutions)
        {
            resolutionStrings.Add($"{res.width} x {res.height}");
        }
        resolutionDropdown.AddOptions(resolutionStrings);

        // Frame Rate Dropdown
        frameRateDropdown.ClearOptions();
        frameRateDropdown.AddOptions(new List<string>
        {
            "30 FPS",
            "60 FPS",
            "120 FPS",
            "144 FPS",
            "Unlimited"
        });
    }

    private void LoadCurrentSettings()
    {
        // Graphics
        windowModeDropdown.value = SettingsManager.Instance.GetWindowMode();
        resolutionDropdown.value = SettingsManager.Instance.GetResolutionIndex();

        int frameRate = SettingsManager.Instance.GetMaxFrameRate();
        frameRateDropdown.value = frameRate switch
        {
            30 => 0,
            60 => 1,
            120 => 2,
            144 => 3,
            _ => 4 // Unlimited
        };

        vSyncToggle.isOn = SettingsManager.Instance.GetVSync();
        renderScaleSlider.value = SettingsManager.Instance.GetRenderScale();
        UpdateRenderScaleText(renderScaleSlider.value);

        // Audio
        masterVolumeSlider.value = SettingsManager.Instance.GetMasterVolume();
        UpdateVolumeText(masterVolumeText, masterVolumeSlider.value);

        sfxVolumeSlider.value = SettingsManager.Instance.GetSFXVolume();
        UpdateVolumeText(sfxVolumeText, sfxVolumeSlider.value);

        musicVolumeSlider.value = SettingsManager.Instance.GetMusicVolume();
        UpdateVolumeText(musicVolumeText, musicVolumeSlider.value);
    }

    private void SetupListeners()
    {
        // Graphics listeners
        windowModeDropdown.onValueChanged.AddListener(OnWindowModeChanged);
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        frameRateDropdown.onValueChanged.AddListener(OnFrameRateChanged);
        vSyncToggle.onValueChanged.AddListener(OnVSyncChanged);
        renderScaleSlider.onValueChanged.AddListener(OnRenderScaleChanged);

        // Audio listeners
        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

        // Button listeners
        if (resetButton != null)
            resetButton.onClick.AddListener(OnResetClicked);

        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseClicked);
    }

    #region Graphics Callbacks

    private void OnWindowModeChanged(int value)
    {
        SettingsManager.Instance.SetWindowMode(value);
    }

    private void OnResolutionChanged(int value)
    {
        SettingsManager.Instance.SetResolution(value);
    }

    private void OnFrameRateChanged(int value)
    {
        int frameRate = value switch
        {
            0 => 30,
            1 => 60,
            2 => 120,
            3 => 144,
            _ => -1 // Unlimited
        };
        SettingsManager.Instance.SetMaxFrameRate(frameRate);
    }

    private void OnVSyncChanged(bool value)
    {
        SettingsManager.Instance.SetVSync(value);
    }

    private void OnRenderScaleChanged(float value)
    {
        SettingsManager.Instance.SetRenderScale(value);
        UpdateRenderScaleText(value);
    }

    private void UpdateRenderScaleText(float value)
    {
        if (renderScaleText != null)
            renderScaleText.text = $"{Mathf.RoundToInt(value * 100)}%";
    }

    #endregion

    #region Audio Callbacks

    private void OnMasterVolumeChanged(float value)
    {
        SettingsManager.Instance.SetMasterVolume(value);
        UpdateVolumeText(masterVolumeText, value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        SettingsManager.Instance.SetSFXVolume(value);
        UpdateVolumeText(sfxVolumeText, value);
    }

    private void OnMusicVolumeChanged(float value)
    {
        SettingsManager.Instance.SetMusicVolume(value);
        UpdateVolumeText(musicVolumeText, value);
    }

    private void UpdateVolumeText(TextMeshProUGUI text, float value)
    {
        if (text != null)
            text.text = $"{Mathf.RoundToInt(value * 100)}%";
    }

    #endregion

    #region Button Callbacks

    private void OnResetClicked()
    {
        SettingsManager.Instance.ResetToDefaults();
        LoadCurrentSettings();
    }

    private void OnCloseClicked()
    {
        gameObject.SetActive(false);
    }

    #endregion

    private void OnDestroy()
    {
        // Clean up listeners
        windowModeDropdown.onValueChanged.RemoveListener(OnWindowModeChanged);
        resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);
        frameRateDropdown.onValueChanged.RemoveListener(OnFrameRateChanged);
        vSyncToggle.onValueChanged.RemoveListener(OnVSyncChanged);
        renderScaleSlider.onValueChanged.RemoveListener(OnRenderScaleChanged);
        masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
        musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
    }
}