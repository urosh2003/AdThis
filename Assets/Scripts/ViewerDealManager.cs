using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ViewerDealManager : MonoBehaviour
{
    public static ViewerDealManager Instance;

    [Header("Deal Generation Settings")]
    [SerializeField] private int dealCount = 50;
    [SerializeField] private float viewersPerMoney = 0.03f;
    [SerializeField] private float noisePercent = 0.3f;
    [SerializeField] private int minCost = 5000;
    [SerializeField] private int maxCost = 200000;
    [SerializeField] private Sprite defaultDealImage;

    [Header("UI References")]
    [SerializeField] private Transform dealCardContainer;

    [SerializeField] private Sprite activeButtonSprite;
    [SerializeField] private Sprite inactiveButtonSprite;
    [SerializeField] private Image streamButtonImage;
    [SerializeField] private Image shopButtonImage;
    
    [SerializeField] private GameObject dealCardPanel;
    [SerializeField] private GameObject dealCardPrefab;
    
    public List<ViewerDeal> Deals { get; private set; } = new List<ViewerDeal>();

    private List<DealCardUI> _cards = new List<DealCardUI>();

    
    [SerializeField] private Image background;
    [SerializeField] private Sprite lightBackground;
    [SerializeField] private Sprite darkBackground;

    [SerializeField] private Image bar;
    [SerializeField] private Sprite lightBar;
    [SerializeField] private Sprite darkBar;
    
    [SerializeField] private Image chatPanel;
    [SerializeField] private Color lightPanelColor;
    [SerializeField] private Color darkPanelColor;
    
    [SerializeField] private AudioSource mainTheme;
    [SerializeField] private AudioSource jimmysTheme;
    
    public static event Action<ViewerDeal> OnDealPurchased;

    private void Awake()
    {
        Instance = this;
        RoundManager.OnStateChanged += CheckForClose;
    }
    public void CheckForClose(RoundState roundState)
    {
        if(roundState!=RoundState.Playing)
        {
            TurnOffDeals();
        }
    }

    public void TurnBackgroundDark()
    {
        background.sprite = darkBackground;
        bar.sprite = darkBar;
        chatPanel.color = darkPanelColor;
        mainTheme.Pause();
        jimmysTheme.Play();
    }

    public void TurnBackgroundLight()
    {
        background.sprite = lightBackground;
        bar.sprite = lightBar;
        chatPanel.color = lightPanelColor;
        mainTheme.UnPause();
        jimmysTheme.Pause();
    }

    public void TurnOnDeals()
    {
        if (RoundManager.Instance.state == RoundState.Playing)
        {
            dealCardPanel.SetActive(true);
            background.sprite = darkBackground;
        }
        
    }

    public void TurnOffDeals()
    {
        dealCardPanel.SetActive(false);
        background.sprite = lightBackground;
    }

    private void Start()
    {
        TurnOffDeals();
        GenerateDeals();
        PopulateUI();
    }

    private void OnEnable()
    {
        GridManager.OnMoneyChanged += OnMoneyChanged;
    }

    private void OnDisable()
    {
        GridManager.OnMoneyChanged -= OnMoneyChanged;
    }

    private void GenerateDeals()
    {
        Deals.Clear();

        for (int i = 0; i < dealCount; i++)
        {
            float cost = UnityEngine.Random.Range(minCost, maxCost + 1);
            cost = Mathf.Round(cost / 1000f) * 1000f;

            float noise = 1f + UnityEngine.Random.Range(-noisePercent, noisePercent);
            float viewers = cost * viewersPerMoney * noise;
            viewers = Mathf.Round(viewers / 10f) * 10f;

            var deal = new ViewerDeal
            {
                dealName = $"Ad Deal #{i + 1}",
                moneyCost = (int)cost,
                viewerAmount = Mathf.Max(10, (int)viewers),
                dealImage = defaultDealImage,
                isPurchased = false
            };
            Deals.Add(deal);
        }

        for (int i = Deals.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (Deals[i], Deals[j]) = (Deals[j], Deals[i]);
        }
    }

    private void PopulateUI()
    {
        foreach (var deal in Deals)
        {
            var cardGO = Instantiate(dealCardPrefab, dealCardContainer);
            var card = cardGO.GetComponent<DealCardUI>();
            card.Setup(deal, this);
            _cards.Add(card);
        }
    }

    public bool CanAfford(ViewerDeal deal)
    {
        return GridManager.Instance.CurrentMoney >= deal.moneyCost;
    }

    public void PurchaseDeal(ViewerDeal deal, DealCardUI card)
    {
        if (deal.isPurchased || !CanAfford(deal))
            return;

        GridManager.Instance.CurrentMoney -= deal.moneyCost;
        GridManager.Instance.CurrentViewers += deal.viewerAmount;
        deal.isPurchased = true;

        card.MarkAsPurchased();
        OnDealPurchased?.Invoke(deal);
    }

    private void OnMoneyChanged(int newMoney)
    {
        foreach (var card in _cards)
        {
            card.UpdateAffordability();
        }
    }
}
