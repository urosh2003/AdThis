public class AntiAdBlockerPowerUp : PowerUp
{
    private float bonusMoneyPerCell;

    public AntiAdBlockerPowerUp(float jimmysCut = 0.12f, float bonusMoneyPerCell = 500f, int duration = 3)
        : base(PaymentMode.JimmysCut, jimmysCut, 0, duration)
    {
        this.powerUpType = PowerUpType.DuringScoring;
        this.bonusMoneyPerCell = bonusMoneyPerCell;
        this.displayName = "Anti Ad-blocker";
        this.description = $"+${bonusMoneyPerCell} per ad cell for {duration} rounds.";
    }

    public override void OnAcquired()
    {
        GridManager.Instance.moneyPerAdPerCell += bonusMoneyPerCell;
    }

    public override void ApplyPowerUp()
    {
        // Modifier is set on acquire and persists until expiry
    }

    public override void OnExpired()
    {
        GridManager.Instance.moneyPerAdPerCell -= bonusMoneyPerCell;
    }
}
