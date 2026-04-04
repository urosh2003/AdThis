using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DealCardUI : MonoBehaviour
{
    [SerializeField] private Image dealImage;
    [SerializeField] private TMP_Text dealNameText;
    [SerializeField] private TMP_Text viewerAmountText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button buyButton;
    [SerializeField] private Image cardBackground;

    [Header("Colors")]
    [SerializeField] private Color affordableColor = Color.white;
    [SerializeField] private Color unaffordableColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
    [SerializeField] private Color purchasedColor = new Color(0.4f, 0.6f, 0.4f, 0.5f);

    private ViewerDeal _deal;
    private ViewerDealManager _manager;

    public void Setup(ViewerDeal deal, ViewerDealManager manager)
    {
        _deal = deal;
        _manager = manager;

        dealNameText.text = deal.dealName;
        viewerAmountText.text = $"+{deal.viewerAmount:N0} viewers";
        priceText.text = $"${deal.moneyCost:N0}";

        if (deal.dealImage != null)
            dealImage.sprite = deal.dealImage;

        buyButton.onClick.AddListener(OnBuyClicked);
        UpdateAffordability();
    }

    private void OnBuyClicked()
    {
        _manager.PurchaseDeal(_deal, this);
    }

    public void UpdateAffordability()
    {
        if (_deal == null || _deal.isPurchased)
            return;

        bool canAfford = _manager.CanAfford(_deal);
        buyButton.interactable = canAfford;
        cardBackground.color = canAfford ? affordableColor : unaffordableColor;
    }

    public void MarkAsPurchased()
    {
        buyButton.interactable = false;
        buyButton.GetComponentInChildren<TMP_Text>().text = "Purchased";
        cardBackground.color = purchasedColor;
    }
}
