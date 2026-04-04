public class BetterGreenMultiplierPowerUp : PowerUp
{
    public float newMultiplier;

    public BetterGreenMultiplierPowerUp(float jimmysCut, float newMultiplier = 3f)
        : base(PaymentMode.JimmysCut, jimmysCut, 0, -1)
    {
        this.powerUpType = PowerUpType.DuringScoring;
        this.newMultiplier = newMultiplier;
        this.displayName = "Better Green Multiplier";
        this.description = "Increases bonus tile multiplier to " + newMultiplier + "x.";
    }

    public override void ApplyPowerUp()
    {
        GridManager gridManager = GridManager.Instance;
        gridManager.bonusMultiplier = newMultiplier;
    }
}
