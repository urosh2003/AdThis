using UnityEngine;
using UnityEngine.Rendering;

public class PostProcessingManager : MonoBehaviour
{
    [SerializeField] private Volume volume;
    [SerializeField] public VolumeProfile defaultVolume;
    [SerializeField] public VolumeProfile jimmyVolume;

    public static PostProcessingManager Instance;

    private void Awake()
    {
        Instance = this;
        SetDefaultProfile();
    }

    private void OnEnable()
    {
        ViewerDealManager.OnBackgroundChanged += OnBackgroundChanged;
    }

    private void OnDisable()
    {
        ViewerDealManager.OnBackgroundChanged -= OnBackgroundChanged;
    }

    private void OnBackgroundChanged(bool isDark)
    {
        if (isDark) SetJimmyProfile();
        else        SetDefaultProfile();
    }

    public void SetJimmyProfile()
    {
        volume.sharedProfile = jimmyVolume;
    }

    public void SetDefaultProfile()
    {
        volume.sharedProfile = defaultVolume;
    }
}