public class CancelablePowerUp : PowerUp
{
    public CancelablePowerUp(float jimmysCut = 0.1f, int duration = 3)
        : base(PaymentMode.JimmysCut, jimmysCut, 0, duration)
    {
        this.powerUpType = PowerUpType.DuringScoring;
        this.displayName = "Cancelable";
        this.description = "2x money but 0.5x viewers for " + duration + " rounds.";
    }

    public override void OnAcquired()
    {
        GridManager.Instance.moneyMultiplier *= 2f;
        GridManager.Instance.viewerMultiplier *= 0.5f;
    }

    public override void ApplyPowerUp()
    {
        // Modifiers are set on acquire and persist until expiry
    }

    public override void OnExpired()
    {
        GridManager.Instance.moneyMultiplier /= 2f;
        GridManager.Instance.viewerMultiplier /= 0.5f;
    }
}
