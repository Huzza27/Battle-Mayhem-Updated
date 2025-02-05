using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;

public class SettingsMenuManager : MonoBehaviour
{

    public static SettingsMenuManager Instance;

    private Canvas settingsCanvas;

    [Header("Graphics")]
    public TMP_Dropdown graphicsQualityDropdown;

    [Header("Audio")]
    public AudioMixer MainMixer;
    public Slider masterSlider, musicSlider, sfxSlider;

    [Header("Screen Scale")]
    public Toggle fullscreenToggle;

    // Keys for PlayerPrefs
    private const string MasterVolumeKey = "MasterVolume";
    private const string MusicVolumeKey = "MusicVolume";
    private const string SFXVolumeKey = "SFXVolume";
    private const string GraphicsQualityKey = "GraphicsQuality";

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        settingsCanvas = GetComponent<Canvas>();

        // Load saved settings
        LoadSettings();
    }

    public void ChangeMasterVolume()
    {
        float volume = masterSlider.value;
        MainMixer.SetFloat("MasterVolume", volume);
        PlayerPrefs.SetFloat(MasterVolumeKey, volume); // Save to PlayerPrefs
    }

    public void ChangeMusicVolume()
    {
        float volume = musicSlider.value;
        MainMixer.SetFloat("MusicVolume", volume);
        PlayerPrefs.SetFloat(MusicVolumeKey, volume); // Save to PlayerPrefs
    }

    public void ChangeSFXVolume()
    {
        float volume = sfxSlider.value;
        MainMixer.SetFloat("SFXVolume", volume);
        PlayerPrefs.SetFloat(SFXVolumeKey, volume); // Save to PlayerPrefs
    }

    public void SetGraphicsQuality()
    {
        int qualityLevel = graphicsQualityDropdown.value;
        QualitySettings.SetQualityLevel(qualityLevel);
        PlayerPrefs.SetInt(GraphicsQualityKey, qualityLevel); // Save to PlayerPrefs
    }

    public void CloseCanvas()
    {
        if (settingsCanvas != null)
        {
            settingsCanvas.enabled = false;
            PlayerPrefs.Save();
        }
    }

    public void OpenCanvas()
    {
        if (settingsCanvas != null)
        {
            settingsCanvas.enabled = true;
        }
    }


    private void LoadSettings()
    {
        // Load Master Volume
        if (PlayerPrefs.HasKey(MasterVolumeKey))
        {
            float masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey);
            MainMixer.SetFloat("MasterVolume", masterVolume);
            masterSlider.value = masterVolume;
        }

        // Load Music Volume
        if (PlayerPrefs.HasKey(MusicVolumeKey))
        {
            float musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey);
            MainMixer.SetFloat("MusicVolume", musicVolume);
            musicSlider.value = musicVolume;
        }

        // Load SFX Volume
        if (PlayerPrefs.HasKey(SFXVolumeKey))
        {
            float sfxVolume = PlayerPrefs.GetFloat(SFXVolumeKey);
            MainMixer.SetFloat("SFXVolume", sfxVolume);
            sfxSlider.value = sfxVolume;
        }

        // Load Graphics Quality
        if (PlayerPrefs.HasKey(GraphicsQualityKey))
        {
            int qualityLevel = PlayerPrefs.GetInt(GraphicsQualityKey);
            QualitySettings.SetQualityLevel(qualityLevel);
            graphicsQualityDropdown.value = qualityLevel;
        }

        if (PlayerPrefs.HasKey("ScreenMode"))
        {
            bool isFullscreen = PlayerPrefs.GetInt("ScreenMode") == 1;
            fullscreenToggle.isOn = isFullscreen;

            if (isFullscreen)
            {
                Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, FullScreenMode.FullScreenWindow);
            }
            else
            {
                Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
                StartCoroutine(EnableWindowResize());
            }
        }
    }

    public void ChangeScreenMode()
    {
        bool isFullscreen = fullscreenToggle.isOn;

        if (isFullscreen)
        {
            // Set Fullscreen Mode (FullScreenWindow ensures it covers the entire screen)
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, FullScreenMode.FullScreenWindow);
            PlayerPrefs.SetInt("ScreenMode", 1);
        }
        else
        {
            int windowedWidth = 1280; // Default Windowed Mode Width
            int windowedHeight = 720; // Default Windowed Mode Height

            // Set the game to Windowed Mode but keep it resizable
            Screen.SetResolution(windowedWidth, windowedHeight, FullScreenMode.Windowed);

            // Ensure the window remains resizable & maximizable
            StartCoroutine(EnableWindowResize());

            PlayerPrefs.SetInt("ScreenMode", 0);
        }

        PlayerPrefs.Save();
    }

    // Coroutine to ensure the window remains resizable after switching to Windowed Mode
    private IEnumerator EnableWindowResize()
    {
        yield return new WaitForSeconds(0.1f); // Small delay to let Unity apply the setting
        Screen.fullScreenMode = FullScreenMode.Windowed; // Ensure the windowed mode is set properly
    }



    private void OnApplicationQuit()
    {
        PlayerPrefs.Save(); // Save PlayerPrefs when the game closes
    }
}
