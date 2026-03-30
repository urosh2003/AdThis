using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Jimmy : MonoBehaviour
{
    [Header("Sprite Settings")]
    [SerializeField] private UnityEngine.Object spritesFolder; // Drag folder here
    
    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 5f;
    [SerializeField] private AnimationCurve alphaCurve;
    [SerializeField] private AnimationCurve scaleCurve;
    [SerializeField] private AnimationCurve rotationCurve;
    [SerializeField] private AnimationCurve positionCurve;
    
    [Header("Flip Settings")]
    [SerializeField] private bool flipAtHalfway = true;
    [SerializeField] private float flipTime = 2.5f;
    [SerializeField] private bool useSmoothFlip = true;
    
    [Header("Effects")]
    [SerializeField] private bool usePulseEffect = true;
    [SerializeField] private float pulseSpeed = 1.5f;
    [SerializeField] private float pulseAmount = 0.05f;
    
    [Header("Color Tint")]
    [SerializeField] private bool useColorTint = true;
    [SerializeField] private Gradient colorGradient;
    
    private Image jimmyImage;
    private RectTransform rectTransform;
    private Coroutine currentFadeCoroutine;
    private Vector2 startPosition;
    private bool hasFlipped = false;
    
    // Sprite management
    private List<Sprite> availableSprites = new List<Sprite>();
    private List<Sprite> usedSprites = new List<Sprite>();
    
    void Start()
    {
        jimmyImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        startPosition = rectTransform.anchoredPosition;
        
        // Load all sprites from the folder
        LoadAllSprites();
        
        // Set initial sprite
        SetRandomSprite();
        
        // Set initial invisible
        if (jimmyImage != null)
        {
            Color startColor = jimmyImage.color;
            startColor.a = 0f;
            jimmyImage.color = startColor;
        }
        
        CreateSubtleCurves();
        
        RoundManager.OnStateChanged += OnRoundStateChanged;
    }
    
    private void LoadAllSprites()
    {
        #if UNITY_EDITOR
        if (spritesFolder == null)
        {
            Debug.LogError("Sprites folder not assigned in Inspector!");
            return;
        }
        
        string folderPath = AssetDatabase.GetAssetPath(spritesFolder);
        
        if (string.IsNullOrEmpty(folderPath))
        {
            Debug.LogError("Invalid folder path!");
            return;
        }
        
        availableSprites.Clear();
        usedSprites.Clear();
        
        // Get all files in the folder
        string[] files = System.IO.Directory.GetFiles(folderPath, "*.png");
        
        foreach (string file in files)
        {
            // Convert Windows path to Unity path
            string unityPath = file.Replace("\\", "/");
            
            // Load the texture
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(unityPath);
            
            if (texture != null)
            {
                // Create sprite from texture
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                availableSprites.Add(sprite);
                Debug.Log($"Loaded: {unityPath}");
            }
            else
            {
                Debug.LogWarning($"Failed to load: {unityPath}");
            }
        }
        
        // Shuffle
        for (int i = 0; i < availableSprites.Count; i++)
        {
            Sprite temp = availableSprites[i];
            int randomIndex = Random.Range(i, availableSprites.Count);
            availableSprites[i] = availableSprites[randomIndex];
            availableSprites[randomIndex] = temp;
        }
        
        Debug.Log($"Loaded {availableSprites.Count} sprites");
        
        #endif
    }
    
    private void SetRandomSprite()
    {
        if (availableSprites.Count == 0)
        {
            // No more available sprites, refill from used sprites
            if (usedSprites.Count > 0)
            {
                availableSprites.AddRange(usedSprites);
                usedSprites.Clear();
                ShuffleList(availableSprites);
                Debug.Log("Refilled sprite pool from used sprites");
            }
            else
            {
                Debug.LogError("No sprites available!");
                return;
            }
        }
        
        // Take the first sprite from available list
        Sprite nextSprite = availableSprites[0];
        availableSprites.RemoveAt(0);
        
        // Add to used sprites
        usedSprites.Add(nextSprite);
        
        // Apply the sprite
        if (jimmyImage != null && nextSprite != null)
        {
            jimmyImage.sprite = nextSprite;
            Debug.Log($"Jimmy now using sprite: {nextSprite.name}");
        }
        else
        {
            Debug.LogError($"Failed to set sprite. Image null? {jimmyImage == null}, Sprite null? {nextSprite == null}");
        }
    }
    
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
    
    private void OnDestroy()
    {
        RoundManager.OnStateChanged -= OnRoundStateChanged;
    }
    
    private void OnRoundStateChanged(RoundState newState)
    {
        if (newState == RoundState.Playing)
        {
            // Pick a new random sprite for this round
            SetRandomSprite();
            
            if (currentFadeCoroutine != null)
                StopCoroutine(currentFadeCoroutine);
            currentFadeCoroutine = StartCoroutine(SubtleFadeOut());
        }
    }
    
    private IEnumerator SubtleFadeOut()
    {
        if (jimmyImage == null) yield break;
        
        // Start visible
        Color currentColor = jimmyImage.color;
        currentColor.a = 1f;
        jimmyImage.color = currentColor;
        
        // Reset flip state
        hasFlipped = false;
        
        // Store original scale (preserve your original scale)
        Vector3 startScale = transform.localScale;
        Quaternion startRotation = transform.localRotation;
        float elapsedTime = 0f;
        float pulseTimer = 0f;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;
            
            // Main curve values
            float alphaValue = alphaCurve.Evaluate(t);
            float scaleValue = scaleCurve.Evaluate(t);
            float rotationValue = rotationCurve.Evaluate(t);
            float positionValue = positionCurve.Evaluate(t);
            
            // Handle flip at halfway point
            if (flipAtHalfway && !hasFlipped && elapsedTime >= flipTime)
            {
                hasFlipped = true;
                
                if (useSmoothFlip)
                {
                    StartCoroutine(SmoothFlip());
                }
                else
                {
                    Vector3 currentScale = transform.localScale;
                    currentScale.x = -Mathf.Abs(currentScale.x);
                    transform.localScale = currentScale;
                }
            }
            
            // Apply fade
            Color newColor = jimmyImage.color;
            newColor.a = Mathf.Lerp(1f, 0f, alphaValue);
            
            // Optional color tint (subtle)
            if (useColorTint && colorGradient != null)
            {
                Color tintColor = colorGradient.Evaluate(t);
                newColor.r = tintColor.r;
                newColor.g = tintColor.g;
                newColor.b = tintColor.b;
                newColor.a = Mathf.Lerp(1f, 0f, alphaValue);
            }
            
            jimmyImage.color = newColor;
            
            // Apply scale (subtle) - preserve flip if it happened
            if (!useSmoothFlip || !hasFlipped)
            {
                transform.localScale = startScale * scaleValue;
            }
            else
            {
                Vector3 currentScale = transform.localScale;
                currentScale.y = startScale.y * scaleValue;
                currentScale.z = startScale.z * scaleValue;
                transform.localScale = currentScale;
            }
            
            // Apply rotation (subtle)
            transform.localRotation = startRotation * Quaternion.Euler(0, 0, rotationValue);
            
            // Apply position wiggle (subtle)
            rectTransform.anchoredPosition = startPosition + new Vector2(0, positionValue);
            
            // Optional subtle pulse effect
            if (usePulseEffect && t < 0.4f)
            {
                pulseTimer += Time.deltaTime * pulseSpeed;
                float pulse = 1f + Mathf.Sin(pulseTimer) * pulseAmount;
                if (!hasFlipped || !useSmoothFlip)
                {
                    transform.localScale = startScale * scaleValue * pulse;
                }
            }
            
            yield return null;
        }
        
        // Final state
        Color finalColor = jimmyImage.color;
        finalColor.a = 0f;
        jimmyImage.color = finalColor;
        rectTransform.anchoredPosition = startPosition;
        
        // Reset scale to original (for next round)
        transform.localScale = startScale;
        
        Debug.Log("Jimmy finished his subtle exit!");
    }
    
    private IEnumerator SmoothFlip()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        float flipDuration = 0.3f;
        float elapsedFlip = 0f;
        
        while (elapsedFlip < flipDuration)
        {
            elapsedFlip += Time.deltaTime;
            float t = elapsedFlip / flipDuration;
            float easedT = Mathf.SmoothStep(0, 1, t);
            transform.localScale = Vector3.Lerp(originalScale, targetScale, easedT);
            yield return null;
        }
        
        transform.localScale = targetScale;
    }
    
    private void CreateSubtleCurves()
    {
        // Alpha: Smooth, natural fade
        alphaCurve = new AnimationCurve();
        alphaCurve.AddKey(0f, 0f);
        alphaCurve.AddKey(0.2f, 0.1f);
        alphaCurve.AddKey(0.5f, 0.35f);
        alphaCurve.AddKey(0.8f, 0.7f);
        alphaCurve.AddKey(1f, 1f);
        
        // Scale: Very subtle shrink
        scaleCurve = new AnimationCurve();
        scaleCurve.AddKey(0f, 1f);
        scaleCurve.AddKey(0.15f, 1.02f);
        scaleCurve.AddKey(0.3f, 1f);
        scaleCurve.AddKey(0.7f, 0.98f);
        scaleCurve.AddKey(1f, 0.25f);
        
        // Rotation: Minimal wobble
        rotationCurve = new AnimationCurve();
        rotationCurve.AddKey(0f, 0f);
        rotationCurve.AddKey(0.2f, 1.5f);
        rotationCurve.AddKey(0.4f, -1f);
        rotationCurve.AddKey(0.6f, 0.5f);
        rotationCurve.AddKey(0.8f, 0f);
        rotationCurve.AddKey(1f, 0f);
        
        // Position: Gentle bounce
        positionCurve = new AnimationCurve();
        positionCurve.AddKey(0f, 0f);
        positionCurve.AddKey(0.2f, 5f);
        positionCurve.AddKey(0.4f, -2f);
        positionCurve.AddKey(0.6f, 1f);
        positionCurve.AddKey(0.8f, 0f);
        positionCurve.AddKey(1f, 0f);
        
        // Color gradient
        if (useColorTint && colorGradient == null)
        {
            colorGradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[3];
            colorKeys[0] = new GradientColorKey(new Color(1f, 1f, 1f), 0f);
            colorKeys[1] = new GradientColorKey(new Color(1f, 0.95f, 0.85f), 0.3f);
            colorKeys[2] = new GradientColorKey(new Color(1f, 0.9f, 0.8f), 0.7f);
            
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0] = new GradientAlphaKey(1f, 0f);
            alphaKeys[1] = new GradientAlphaKey(0f, 1f);
            
            colorGradient.SetKeys(colorKeys, alphaKeys);
        }
    }
    
    public void TriggerJimmyAppearance()
    {
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }
        currentFadeCoroutine = StartCoroutine(SubtleFadeOut());
    }
}