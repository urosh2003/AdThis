public class UncancelablePowerUp : PowerUp
{
    public UncancelablePowerUp(float jimmysCut = 0.15f, int duration = 2)
        : base(PaymentMode.JimmysCut, jimmysCut, 0, duration)
    {
        this.powerUpType = PowerUpType.DuringScoring;
        this.displayName = "Uncancelable";
        this.description = "Don't lose viewers for " + duration + " rounds.";
    }

    public override void OnAcquired()
    {
        GridManager.Instance.preventViewerLoss = true;
    }

    public override void ApplyPowerUp()
    {
        GridManager.Instance.preventViewerLoss = true;
    }

    public override void OnExpired()
    {
        GridManager.Instance.preventViewerLoss = false;
    }
}
