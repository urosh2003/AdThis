using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpSelectionUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panel;

    [Header("Option 1")]
    [SerializeField] private GameObject option1Root;
    [SerializeField] private TMP_Text option1Name;
    [SerializeField] private TMP_Text option1Description;
    [SerializeField] private TMP_Text option1Cost;
    [SerializeField] private Button option1Button;

    [Header("Option 2")]
    [SerializeField] private GameObject option2Root;
    [SerializeField] private TMP_Text option2Name;
    [SerializeField] private TMP_Text option2Description;
    [SerializeField] private TMP_Text option2Cost;
    [SerializeField] private Button option2Button;

    [Header("Skip")]
    [SerializeField] private Button skipButton;

    private Action<IPowerUp> _onSelected;
    private List<IPowerUp> _currentChoices;

    void Awake()
    {
        panel.SetActive(false);
    }

    public void Show(List<IPowerUp> choices, Action<IPowerUp> onSelected)
    {
        _currentChoices = choices;
        _onSelected = onSelected;

        if (choices.Count >= 1)
        {
            option1Root.SetActive(true);
            option1Name.text = choices[0].displayName;
            option1Description.text = choices[0].description;
            option1Cost.text = $"Jimmy's Cut: {choices[0].jimmysCut * 100:0}%";
        }
        else
        {
            option1Root.SetActive(false);
        }

        if (choices.Count >= 2)
        {
            option2Root.SetActive(true);
            option2Name.text = choices[1].displayName;
            option2Description.text = choices[1].description;
            option2Cost.text = $"Jimmy's Cut: {choices[1].jimmysCut * 100:0}%";
        }
        else
        {
            option2Root.SetActive(false);
        }

        option1Button.onClick.RemoveAllListeners();
        option1Button.onClick.AddListener(() => OnChoose(0));
        option2Button.onClick.RemoveAllListeners();
        option2Button.onClick.AddListener(() => OnChoose(1));
        skipButton.onClick.RemoveAllListeners();
        skipButton.onClick.AddListener(OnSkip);

        panel.SetActive(true);
        Time.timeScale = 0f;
    }

    private void OnChoose(int index)
    {
        var selected = _currentChoices[index];
        Hide();
        _onSelected?.Invoke(selected);
    }

    private void OnSkip()
    {
        Hide();
        _onSelected?.Invoke(null);
    }

    private void Hide()
    {
        panel.SetActive(false);
        Time.timeScale = 1f;
    }
}
