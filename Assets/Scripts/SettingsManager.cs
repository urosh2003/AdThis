using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    // Singleton instance
    public static SettingsManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject settingsPanel;
    
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Volume Snapping")]
    [SerializeField] private float volumeSnapIncrement = 0.05f;

    [Header("Volume Settings")]
    [Range(0.0001f, 1f)]
    public float masterVolume = 1f;
    [Range(0.0001f, 1f)]
    public float sfxVolume = 1f;
    [Range(0.0001f, 1f)]
    public float musicVolume = 1f;

    private const string MASTER_VOLUME_PARAM = "MasterVolume";
    private const string SFX_VOLUME_PARAM = "SFXVolume";
    private const string MUSIC_VOLUME_PARAM = "MusicVolume";
    
    private bool isSettingsOpen = false;
    private float previousTimeScale = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    
    private void Start()
    {
        LoadSettings();
        ApplyAllVolumes();
        
        // Find settings panel if not assigned
        if (settingsPanel == null)
        {
            settingsPanel = GameObject.Find("SettingsPanel");
        }
        
        if (settingsPanel != null)
        {
            var settingsMenuUI = settingsPanel.GetComponent<SettingsMenuUI>();
            if (settingsMenuUI != null)
            {
                settingsMenuUI.InitializeSliders();
            }
            settingsPanel.SetActive(false);
            isSettingsOpen = false;
        }
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSettingsMenu();
        }
    }
    
    public void ToggleSettingsMenu()
    {
        if (isSettingsOpen)
        {
            CloseSettings();
        }
        else
        {
            OpenSettings();
        }
    }
    
    public void OpenSettings()
    {
        if (settingsPanel == null)
        {
            Debug.LogWarning("Settings Panel not assigned in SettingsManager!");
            return;
        }
        
        settingsPanel.SetActive(true);
        isSettingsOpen = true;
        
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        
        Debug.Log("Settings opened");
    }
    
    public void CloseSettings()
    {
        if (settingsPanel == null) return;
        
        settingsPanel.SetActive(false);
        isSettingsOpen = false;
        
        Time.timeScale = previousTimeScale;
        
        Debug.Log("Settings closed");
    }
    
    public bool IsSettingsOpen()
    {
        return isSettingsOpen;
    }

    public void SetMasterVolume(float volume)
    {
        float snappedVolume = Mathf.Round(volume / volumeSnapIncrement) * volumeSnapIncrement;
        snappedVolume = Mathf.Clamp(snappedVolume, 0.0001f, 1f);
        
        masterVolume = snappedVolume;
        ApplyVolume(MASTER_VOLUME_PARAM, masterVolume);
        SaveSettings();
    }
    
    public void SetSFXVolume(float volume)
    {
        float snappedVolume = Mathf.Round(volume / volumeSnapIncrement) * volumeSnapIncrement;
        snappedVolume = Mathf.Clamp(snappedVolume, 0.0001f, 1f);
        
        sfxVolume = snappedVolume;
        ApplyVolume(SFX_VOLUME_PARAM, sfxVolume);
        SaveSettings();
    }
    
    public void SetMusicVolume(float volume)
    {
        float snappedVolume = Mathf.Round(volume / volumeSnapIncrement) * volumeSnapIncrement;
        snappedVolume = Mathf.Clamp(snappedVolume, 0.0001f, 1f);
        
        musicVolume = snappedVolume;
        ApplyVolume(MUSIC_VOLUME_PARAM, musicVolume);
        SaveSettings();
    }

    private void ApplyVolume(string parameterName, float linearVolume)
    {
        if (audioMixer == null)
        {
            Debug.LogWarning("AudioMixer not assigned in SettingsManager!");
            return;
        }

        float decibels = Mathf.Log10(Mathf.Max(linearVolume, 0.0001f)) * 20f;
        audioMixer.SetFloat(parameterName, decibels);
    }

    private void ApplyAllVolumes()
    {
        ApplyVolume(MASTER_VOLUME_PARAM, masterVolume);
        ApplyVolume(SFX_VOLUME_PARAM, sfxVolume);
        ApplyVolume(MUSIC_VOLUME_PARAM, musicVolume);
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.Save();
        Debug.Log("Settings saved!");
    }

    public void LoadSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        Debug.Log("Settings loaded!");
    }

    public void ResetToDefaults()
    {
        masterVolume = 0.75f;
        sfxVolume = 0.75f;
        musicVolume = 0.75f;
        ApplyAllVolumes();
        SaveSettings();
        
        // Update UI if settings panel exists
        if (settingsPanel != null)
        {
            var settingsMenuUI = settingsPanel.GetComponent<SettingsMenuUI>();
            if (settingsMenuUI != null)
            {
                settingsMenuUI.InitializeSliders();
            }
        }
        Debug.Log("Settings reset to defaults!");
    }

    private void OnDestroy()
    {
        Debug.Log($"SettingsManager on {gameObject.scene.name} is being destroyed");
    }

    public float GetMasterVolume() => masterVolume;
    public float GetSFXVolume() => sfxVolume;
    public float GetMusicVolume() => musicVolume;
}