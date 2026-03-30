using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StreamImageManager : MonoBehaviour
{
    [SerializeField] private SpriteRenderer displayImage;
    [SerializeField] private List<Sprite> streamImages;
    [SerializeField] private float switchInterval = 0.5f;

    private int currentStreamIndex = -1;
    private Coroutine switchCoroutine;

    void Start()
    {
        displayImage.enabled = false;
        RoundManager.OnStateChanged += SetupStream;
    }
    
    private void SetupStream(RoundState state)
    {
        if (state == RoundState.BetweenRounds)
        {
            displayImage.enabled = true;
        }
        else
        {
            displayImage.enabled = false;
        }
    }

    private void OnDestroy()
    {
        RoundManager.OnStateChanged -= SetupStream;
    }

    public void SetStream(int zoneIndex)
    {
        // Zone index is 1-10 based on GridManager.GenerateZones()
        // Stream 1 uses images 0 and 1, Stream 2 uses 2 and 3, etc.
        currentStreamIndex = zoneIndex - 1;

        if (switchCoroutine != null)
        {
            StopCoroutine(switchCoroutine);
        }

        if (streamImages == null || streamImages.Count < (currentStreamIndex * 2 + 2))
        {
            Debug.LogWarning("Not enough stream images for zone: " + zoneIndex);
            return;
        }

        switchCoroutine = StartCoroutine(SwitchStreamSprites());
    }

    private IEnumerator SwitchStreamSprites()
    {
        int spriteIndex = 0;
        int baseIndex = currentStreamIndex * 2;

        while (true)
        {
            if (displayImage != null)
            {
                displayImage.sprite = streamImages[baseIndex + spriteIndex];
            }

            spriteIndex = (spriteIndex + 1) % 2;
            yield return new WaitForSeconds(switchInterval);
        }
    }
}
