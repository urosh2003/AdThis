using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class VolumeSlider : MonoBehaviour
{
    [Header("Snapping Settings")]
    [Tooltip("The increment value for each notch (0.05 = 5% increments)")]
    [SerializeField] private float snapIncrement = 0.05f;
    
    [Header("Optional: Visual Feedback")]
    [SerializeField] private Text valueDisplayText;
    [SerializeField] private string valuePrefix = "";
    [SerializeField] private string valueSuffix = "%";
    
    private Slider slider;
    private bool isUpdating = false;
    
    private void Awake()
    {
        slider = GetComponent<Slider>();
        
        // Add listener to handle value changes
        slider.onValueChanged.AddListener(OnSliderValueChanged);
    }
    
    private void Start()
    {
        // Snap initial value to the nearest increment
        SnapValueToIncrement(slider.value);
    }
    
    private void OnSliderValueChanged(float value)
    {
        if (!isUpdating)
        {
            // Snap the value to the nearest increment
            float snappedValue = Mathf.Round(value / snapIncrement) * snapIncrement;
            
            // Clamp to min/max
            snappedValue = Mathf.Clamp(snappedValue, slider.minValue, slider.maxValue);
            
            // Only update if the value changed
            if (!Mathf.Approximately(snappedValue, slider.value))
            {
                isUpdating = true;
                slider.value = snappedValue;
                isUpdating = false;
            }
            
            // Update display text
            UpdateDisplayText(snappedValue);
        }
    }
    
    private void SnapValueToIncrement(float value)
    {
        float snappedValue = Mathf.Round(value / snapIncrement) * snapIncrement;
        snappedValue = Mathf.Clamp(snappedValue, slider.minValue, slider.maxValue);
        
        if (!Mathf.Approximately(snappedValue, slider.value))
        {
            isUpdating = true;
            slider.value = snappedValue;
            isUpdating = false;
        }
        
        UpdateDisplayText(snappedValue);
    }
    
    private void UpdateDisplayText(float value)
    {
        if (valueDisplayText != null)
        {
            // Convert to percentage (0-100)
            int percentage = Mathf.RoundToInt(value * 100f);
            valueDisplayText.text = $"{valuePrefix}{percentage}{valueSuffix}";
        }
    }
    
    // Public method to manually set value with snapping
    public void SetValue(float value)
    {
        float snappedValue = Mathf.Round(value / snapIncrement) * snapIncrement;
        snappedValue = Mathf.Clamp(snappedValue, slider.minValue, slider.maxValue);
        
        isUpdating = true;
        slider.value = snappedValue;
        isUpdating = false;
        
        UpdateDisplayText(snappedValue);
    }
    
    // Optional: Called when the slider is released (for haptic feedback)
    public void OnPointerUp()
    {
        // Add haptic feedback for mobile
        #if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
        #endif
        
        // Optional: Play a click sound
        // AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position);
    }
}