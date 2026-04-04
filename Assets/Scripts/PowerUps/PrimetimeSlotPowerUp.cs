public class PrimetimeSlotPowerUp : PowerUp
{
    public PrimetimeSlotPowerUp(int moneyCost = 125000)
        : base(PaymentMode.MoneyCost, 0f, moneyCost, 1)
    {
        this.powerUpType = PowerUpType.DuringScoring;
        this.displayName = "Primetime Slot";
        this.description = "Double the viewers gained this round.";
    }

    public override void OnAcquired()
    {
        GridManager.Instance.viewerMultiplier *= 2f;
    }

    public override void ApplyPowerUp()
    {
        // Modifier is set on acquire and persists until expiry
    }

    public override void OnExpired()
    {
        GridManager.Instance.viewerMultiplier /= 2f;
    }
}
