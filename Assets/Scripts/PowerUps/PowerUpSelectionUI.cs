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

    [Header("Jimmy World Timer")]
    [SerializeField] private float jimmyWorldDuration = 15f;
    [SerializeField] private Image timerImage;

    private Action<PowerUp> _onSelected;
    private List<PowerUp> _currentChoices;
    private float _selectionTimeRemaining;
    private bool _timerActive;

    void Awake()
    {
        panel.SetActive(false);
    }

    void Update()
    {
        if (!_timerActive) return;

        _selectionTimeRemaining -= Time.deltaTime;

        if (timerImage != null)
            timerImage.fillAmount = _selectionTimeRemaining / jimmyWorldDuration;

        RoundManager.Instance.timeRemaining = _selectionTimeRemaining;

        if (_selectionTimeRemaining <= 0f)
        {
            _selectionTimeRemaining = 0f;
            _timerActive = false;
            var randomChoice = _currentChoices[UnityEngine.Random.Range(0, _currentChoices.Count)];
            Hide();
            _onSelected?.Invoke(randomChoice);
        }
    }

    public void Show(List<PowerUp> choices, Action<PowerUp> onSelected)
    {
        ChatCommentManager.Instance.HideChat();
        ViewerDealManager.Instance.TurnBackgroundDark();
        _currentChoices = choices;
        _onSelected = onSelected;

        if (choices.Count >= 1)
        {
            option1Root.SetActive(true);
            option1Name.text = choices[0].displayName;
            option1Description.text = choices[0].description;
            option1Cost.text = choices[0].paymentMode == PaymentMode.JimmysCut
                ? $"Jimmy's Cut: {choices[0].jimmysCut * 100:0}%"
                : $"Cost: ${choices[0].moneyCost:N0}";
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
            option2Cost.text = choices[1].paymentMode == PaymentMode.JimmysCut
                ? $"Jimmy's Cut: {choices[1].jimmysCut * 100:0}%"
                : $"Cost: ${choices[1].moneyCost:N0}";
        }
        else
        {
            option2Root.SetActive(false);
        }

        option1Button.onClick.RemoveAllListeners();
        option1Button.onClick.AddListener(() => OnChoose(0));
        option2Button.onClick.RemoveAllListeners();
        option2Button.onClick.AddListener(() => OnChoose(1));
        //skipButton.onClick.RemoveAllListeners();
        //skipButton.onClick.AddListener(OnSkip);

        panel.SetActive(true);

        _selectionTimeRemaining = jimmyWorldDuration;
        _timerActive = true;
        RoundManager.Instance.timeRemaining = jimmyWorldDuration;
    }

    private void OnChoose(int index)
    {
        if (!_timerActive) return;
        _timerActive = false;
        var selected = _currentChoices[index];
        RoundManager.Instance.DrainTimerThen(() =>
        {
            Hide();
            _onSelected?.Invoke(selected);
        });
    }

    private void OnSkip()
    {
        if (!_timerActive) return;
        _timerActive = false;
        RoundManager.Instance.DrainTimerThen(() =>
        {
            Hide();
            _onSelected?.Invoke(null);
        });
    }

    private void Hide()
    {
        _timerActive = false;
        panel.SetActive(false);
    }
}
