using UnityEngine;

public class PrimetimeSlotPowerUp : PowerUp
{
    private int savedViewers;

    public PrimetimeSlotPowerUp(int moneyCost = 125000)
        : base(PaymentMode.MoneyCost, 0f, moneyCost, 1)
    {
        this.powerUpType = PowerUpType.OnRoundStart;
        this.displayName = "Primetime Slot";
        this.description = "Double your viewers for 1 round.";
    }

    public override void OnAcquired()
    {
        savedViewers = GridManager.Instance.CurrentViewers;
        GridManager.Instance.CurrentViewers = savedViewers * 2;
    }

    public override void ApplyPowerUp() { }

    public override void OnExpired()
    {
        // CurrentViewers = savedViewers*2 + roundGainLoss
        // Restore to savedViewers + roundGainLoss = CurrentViewers - savedViewers
        GridManager.Instance.CurrentViewers = Mathf.Max(0, GridManager.Instance.CurrentViewers - savedViewers);
    }
}
