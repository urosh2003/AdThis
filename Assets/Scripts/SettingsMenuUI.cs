using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button mainMenuButton;
    
    [Header("Volume Snapping")]
    [SerializeField] private float snapIncrement = 0.05f;
    
    [Header("Optional UI Feedback")]
    [SerializeField] private Text masterVolumeValueText;
    [SerializeField] private Text sfxVolumeValueText;
    [SerializeField] private Text musicVolumeValueText;
    
    private void Start()
    {
        // Check if SettingsManager exists
        if (SettingsManager.Instance == null)
        {
            Debug.LogError("SettingsManager instance not found!");
            return;
        }
        
        // Initialize sliders with current values
        // InitializeSliders();
        
        // Add listeners to sliders
        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        
        // Add listeners for when the slider is released (to snap)
        AddSliderReleaseListeners();
        
        // Add button listeners
        if (resetButton != null)
            resetButton.onClick.AddListener(OnResetClicked);
        
        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseClicked);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnCloseClicked);
    }

    private void OnDestroy()
    {
        masterVolumeSlider.onValueChanged.RemoveAllListeners();
        sfxVolumeSlider.onValueChanged.RemoveAllListeners();
        musicVolumeSlider.onValueChanged.RemoveAllListeners();
        
        // Add button listeners
        if (resetButton != null)
            resetButton.onClick.RemoveAllListeners();
        
        if (closeButton != null)
            closeButton.onClick.RemoveAllListeners();

        if (mainMenuButton != null)
            mainMenuButton.onClick.RemoveAllListeners();
    }
    
    public void InitializeSliders()
    {
        // Set slider values from SettingsManager and snap them
        masterVolumeSlider.value = SnapValue(SettingsManager.Instance.GetMasterVolume());
        sfxVolumeSlider.value = SnapValue(SettingsManager.Instance.GetSFXVolume());
        musicVolumeSlider.value = SnapValue(SettingsManager.Instance.GetMusicVolume());
        
        UpdateVolumeTexts();
    }
    
    private void AddSliderReleaseListeners()
    {
        // Get the slider components and add a listener for when dragging ends
        AddSliderPointerUp(masterVolumeSlider);
        AddSliderPointerUp(sfxVolumeSlider);
        AddSliderPointerUp(musicVolumeSlider);
    }
    
    private void RemoveSliderReleaseListeners()
    {
        // Get the slider components and add a listener for when dragging ends
        RemoemoveSliderPointerUp(masterVolumeSlider);
        RemoemoveSliderPointerUp(sfxVolumeSlider);
        RemoemoveSliderPointerUp(musicVolumeSlider);
    }
    
    private void RemoemoveSliderPointerUp(Slider slider)
    {
        // Create a trigger for when the slider is released
        var trigger = slider.gameObject.GetComponent<SliderPointerTrigger>();
        
        trigger.OnPointerUpEvent -= () => SnapSliderValue(slider);
    }
    
    private void AddSliderPointerUp(Slider slider)
    {
        // Create a trigger for when the slider is released
        var trigger = slider.gameObject.GetComponent<SliderPointerTrigger>();
        if (trigger == null)
            trigger = slider.gameObject.AddComponent<SliderPointerTrigger>();
        
        trigger.OnPointerUpEvent += () => SnapSliderValue(slider);
    }
    
    private void SnapSliderValue(Slider slider)
    {
        float snappedValue = SnapValue(slider.value);
        if (!Mathf.Approximately(snappedValue, slider.value))
        {
            slider.value = snappedValue;
        }
    }
    
    private float SnapValue(float value)
    {
        return Mathf.Round(value / snapIncrement) * snapIncrement;
    }
    
    private void OnMasterVolumeChanged(float value)
    {
        // Don't snap during drag, let the slider move smoothly
        SettingsManager.Instance.SetMasterVolume(value);
        UpdateVolumeTexts();
    }
    
    private void OnSFXVolumeChanged(float value)
    {
        SettingsManager.Instance.SetSFXVolume(value);
        UpdateVolumeTexts();
    }
    
    private void OnMusicVolumeChanged(float value)
    {
        SettingsManager.Instance.SetMusicVolume(value);
        UpdateVolumeTexts();
    }
    
    private void OnResetClicked()
    {
        SettingsManager.Instance.ResetToDefaults();
        
        // Update UI to reflect defaults with snapping
        masterVolumeSlider.value = SnapValue(SettingsManager.Instance.GetMasterVolume());
        sfxVolumeSlider.value = SnapValue(SettingsManager.Instance.GetSFXVolume());
        musicVolumeSlider.value = SnapValue(SettingsManager.Instance.GetMusicVolume());
        
        UpdateVolumeTexts();
    }
    
    private void OnCloseClicked()
    {
        SettingsManager.Instance.CloseSettings();
    }

    private void OnMainMenuClicked()
    {
        // You can add any additional logic here if needed before closing settings
        SettingsManager.Instance.CloseSettings();
    }
    
    private void UpdateVolumeTexts()
    {
        if (masterVolumeValueText != null)
            masterVolumeValueText.text = Mathf.RoundToInt(SettingsManager.Instance.GetMasterVolume() * 100) + "%";
        
        if (sfxVolumeValueText != null)
            sfxVolumeValueText.text = Mathf.RoundToInt(SettingsManager.Instance.GetSFXVolume() * 100) + "%";
        
        if (musicVolumeValueText != null)
            musicVolumeValueText.text = Mathf.RoundToInt(SettingsManager.Instance.GetMusicVolume() * 100) + "%";
    }
}

// Helper script to detect when a slider is released
public class SliderPointerTrigger : UnityEngine.EventSystems.EventTrigger
{
    public System.Action OnPointerUpEvent;
    
    public override void OnPointerUp(UnityEngine.EventSystems.PointerEventData data)
    {
        OnPointerUpEvent?.Invoke();
    }
}