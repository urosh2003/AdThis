public class FixedMoneyPerAdCellPowerUp : PowerUp
{
    public int moneyPerAdPerCell;

    public FixedMoneyPerAdCellPowerUp(float jimmysCut, int moneyPerAd)
        : base(PaymentMode.JimmysCut, jimmysCut, 0, -1)
    {
        this.powerUpType = PowerUpType.DuringScoring;
        this.moneyPerAdPerCell = moneyPerAd;
        this.displayName = "Fixed Ad Revenue";
        this.description = $"+${moneyPerAd} per ad cell permanently.";
    }

    public override void ApplyPowerUp()
    {
        GridManager gridManager = GridManager.Instance;
        gridManager.moneyPerAdPerCell = moneyPerAdPerCell;
    }
}
