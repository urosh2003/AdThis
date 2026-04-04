public class PrimetimeSlotPowerUp : PowerUp
{
    public PrimetimeSlotPowerUp(int moneyCost = 80000)
        : base(PaymentMode.MoneyCost, 0f, moneyCost, 1)
    {
        this.powerUpType = PowerUpType.OnRoundStart;
        this.displayName = "Primetime Slot";
        this.description = "Double your viewers for 1 round.";
    }

    public override void ApplyPowerUp()
    {
        GridManager.Instance.CurrentViewers *= 2;
    }
}
