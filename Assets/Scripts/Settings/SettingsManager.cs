using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using System.Linq;
#if UNITY_URP
using UnityEngine.Rendering.Universal;
#endif

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;

    // Settings data
    private SettingsData settings;

    private void Awake()
    {
        // Singleton pattern with DontDestroyOnLoad
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSettings();
        ApplyAllSettings();
    }

    #region Graphics Settings

    public void SetWindowMode(int mode)
    {
        settings.windowMode = mode;

        switch (mode)
        {
            case 0: // Fullscreen
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                break;
            case 1: // Borderless Window
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;
            case 2: // Windowed
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
        }

        SaveSettings();
    }

    public void SetResolution(int resolutionIndex)
    {
        settings.resolutionIndex = resolutionIndex;

        Resolution[] resolutions = Screen.resolutions;
        if (resolutionIndex >= 0 && resolutionIndex < resolutions.Length)
        {
            Resolution res = resolutions[resolutionIndex];
            Screen.SetResolution(res.width, res.height, Screen.fullScreenMode);
        }

        SaveSettings();
    }

    public void SetMaxFrameRate(int frameRate)
    {
        settings.maxFrameRate = frameRate;

        if (frameRate <= 0)
        {
            Application.targetFrameRate = -1; // Unlimited
        }
        else
        {
            Application.targetFrameRate = frameRate;
        }

        SaveSettings();
    }

    public void SetVSync(bool enabled)
    {
        settings.vSyncEnabled = enabled;
        QualitySettings.vSyncCount = enabled ? 1 : 0;
        SaveSettings();
    }

    public void SetRenderScale(float scale)
    {
        settings.renderScale = Mathf.Clamp(scale, 0.5f, 2.0f);

#if UNITY_URP
        // For URP - Access the pipeline asset
        UniversalRenderPipelineAsset urpAsset = 
            UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        
        if (urpAsset != null)
        {
            // Set render scale on the URP asset
            // Note: This requires accessing the serialized property
            var serializedObject = new UnityEditor.SerializedObject(urpAsset);
            var renderScaleProp = serializedObject.FindProperty("m_RenderScale");
            if (renderScaleProp != null)
            {
                renderScaleProp.floatValue = settings.renderScale;
                serializedObject.ApplyModifiedProperties();
            }
        }
#else
        // For Built-in Render Pipeline
        // Option 1: Use Camera's render target (performance impact)
        ApplyRenderScaleToCamera(settings.renderScale);

        // Option 2: Adjust resolution (simpler but less flexible)
        // This is handled in the SetResolution method instead
#endif

        SaveSettings();
    }

    // Helper method for built-in pipeline
    private void ApplyRenderScaleToCamera(float scale)
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        // Calculate scaled resolution
        int scaledWidth = Mathf.RoundToInt(Screen.width * scale);
        int scaledHeight = Mathf.RoundToInt(Screen.height * scale);

        // Create or update render texture
        if (mainCam.targetTexture == null ||
            mainCam.targetTexture.width != scaledWidth ||
            mainCam.targetTexture.height != scaledHeight)
        {
            if (mainCam.targetTexture != null)
            {
                mainCam.targetTexture.Release();
                Destroy(mainCam.targetTexture);
            }

            RenderTexture rt = new RenderTexture(scaledWidth, scaledHeight, 24);
            rt.antiAliasing = QualitySettings.antiAliasing;
            mainCam.targetTexture = rt;
        }
    }

    #endregion

    #region Audio Settings

    public void SetMasterVolume(float volume)
    {
        settings.masterVolume = Mathf.Clamp01(volume);

        if (audioMixer != null)
        {
            // Convert to decibels (0-1 → -80dB to 0dB)
            float dB = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
            audioMixer.SetFloat("MasterVolume", dB);
        }
        else
        {
            AudioListener.volume = volume;
        }

        SaveSettings();
    }

    public void SetSFXVolume(float volume)
    {
        settings.sfxVolume = Mathf.Clamp01(volume);

        if (audioMixer != null)
        {
            float dB = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
            audioMixer.SetFloat("SFXVolume", dB);
        }

        SaveSettings();
    }

    public void SetMusicVolume(float volume)
    {
        settings.musicVolume = Mathf.Clamp01(volume);

        if (audioMixer != null)
        {
            float dB = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
            audioMixer.SetFloat("MusicVolume", dB);
        }

        SaveSettings();
    }

    #endregion

    #region Getters

    public int GetWindowMode() => settings.windowMode;
    public int GetResolutionIndex() => settings.resolutionIndex;
    public int GetMaxFrameRate() => settings.maxFrameRate;
    public bool GetVSync() => settings.vSyncEnabled;
    public float GetRenderScale() => settings.renderScale;
    public float GetMasterVolume() => settings.masterVolume;
    public float GetSFXVolume() => settings.sfxVolume;
    public float GetMusicVolume() => settings.musicVolume;

    public Resolution[] GetAvailableResolutions()
    {
        // Filter to unique resolutions and reasonable refresh rates
        return Screen.resolutions
            .Where(r => r.refreshRate >= 60)
            .GroupBy(r => new { r.width, r.height })
            .Select(g => g.First())
            .ToArray();
    }

    #endregion

    #region Save/Load

    private void SaveSettings()
    {
        string json = JsonUtility.ToJson(settings, true);
        PlayerPrefs.SetString("GameSettings", json);
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        if (PlayerPrefs.HasKey("GameSettings"))
        {
            string json = PlayerPrefs.GetString("GameSettings");
            settings = JsonUtility.FromJson<SettingsData>(json);
        }
        else
        {
            // Default settings
            settings = new SettingsData
            {
                windowMode = 0,
                resolutionIndex = Screen.resolutions.Length - 1,
                maxFrameRate = 60,
                vSyncEnabled = true,
                renderScale = 1.0f,
                masterVolume = 1.0f,
                sfxVolume = 1.0f,
                musicVolume = 1.0f
            };
            SaveSettings();
        }
    }

    private void ApplyAllSettings()
    {
        SetWindowMode(settings.windowMode);
        SetResolution(settings.resolutionIndex);
        SetMaxFrameRate(settings.maxFrameRate);
        SetVSync(settings.vSyncEnabled);
        SetRenderScale(settings.renderScale);
        SetMasterVolume(settings.masterVolume);
        SetSFXVolume(settings.sfxVolume);
        SetMusicVolume(settings.musicVolume);
    }

    public void ResetToDefaults()
    {
        settings = new SettingsData
        {
            windowMode = 0,
            resolutionIndex = Screen.resolutions.Length - 1,
            maxFrameRate = 60,
            vSyncEnabled = true,
            renderScale = 1.0f,
            masterVolume = 1.0f,
            sfxVolume = 1.0f,
            musicVolume = 1.0f
        };
        ApplyAllSettings();
        SaveSettings();
    }

    #endregion
}

[System.Serializable]
public class SettingsData
{
    // Graphics
    public int windowMode; // 0=Fullscreen, 1=Borderless, 2=Windowed
    public int resolutionIndex;
    public int maxFrameRate;
    public bool vSyncEnabled;
    public float renderScale;

    // Audio
    public float masterVolume;
    public float sfxVolume;
    public float musicVolume;
}