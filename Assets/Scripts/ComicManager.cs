using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ComicManager : MonoBehaviour
{
    [SerializeField] private List<Image> comicPanels;
    [SerializeField] private TMP_Text clickToStartText;
    [SerializeField] private GameObject button;
    [SerializeField] private float fadeSpeed = 1f;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private int _currentPanelIndex = 0;
    private bool _isFading = false;
    private bool _allPanelsRevealed = false;

    void Start()
    {
        // Initialize panels to fully opaque (blocking what's underneath)
        foreach (var panel in comicPanels)
        {
            if (panel != null)
            {
                Color c = panel.color;
                c.a = 1;
                panel.color = c;
            }
        }

        if (button != null)
            button.gameObject.SetActive(false);

        if (comicPanels.Count > 0)
        {
            StartCoroutine(FadeOutPanel(_currentPanelIndex));
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }

    private void HandleClick()
    {
        if (_allPanelsRevealed)
        {
            SceneManager.LoadScene(mainMenuSceneName);
            return;
        }

        if (_isFading)
        {
            // Instantly reveal the current panel (make it disappear)
            StopAllCoroutines();
            RevealPanelInstantly(_currentPanelIndex);
            _isFading = false;
            
            // Move to next panel if available
            _currentPanelIndex++;
            if (_currentPanelIndex < comicPanels.Count)
            {
                StartCoroutine(FadeOutPanel(_currentPanelIndex));
            }
            else
            {
                FinishComic();
            }
        }
        else
        {
            // If we are between panels or at the end
            if (_currentPanelIndex < comicPanels.Count)
            {
                // This case might happen if we reached here after an instant reveal
                // or if we add a delay between panels.
                // For now, if we click and it's not fading, we just wait for the next fade or handle end.
            }
        }
    }

    private IEnumerator FadeOutPanel(int index)
    {
        _isFading = true;
        Image panel = comicPanels[index];
        if (panel == null)
        {
            _isFading = false;
            yield break;
        }

        float alpha = panel.color.a;
        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            Color c = panel.color;
            c.a = Mathf.Clamp01(alpha);
            panel.color = c;
            yield return null;
        }

        _isFading = false;
        _currentPanelIndex++;

        if (_currentPanelIndex < comicPanels.Count)
        {
            yield return StartCoroutine(FadeOutPanel(_currentPanelIndex));
        }
        else
        {
            FinishComic();
        }
    }

    private void RevealPanelInstantly(int index)
    {
        if (index >= 0 && index < comicPanels.Count && comicPanels[index] != null)
        {
            Color c = comicPanels[index].color;
            c.a = 0f;
            comicPanels[index].color = c;
        }
    }

    private void FinishComic()
    {
        _allPanelsRevealed = true;
        if (button != null)
            button.gameObject.SetActive(true);
    }
}
