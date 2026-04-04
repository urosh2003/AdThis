public class PrettyPrivilegePowerUp : PowerUp
{
    public PrettyPrivilegePowerUp(float jimmysCut = 0.12f, int duration = 5)
        : base(PaymentMode.JimmysCut, jimmysCut, 0, duration)
    {
        this.powerUpType = PowerUpType.DuringScoring;
        this.displayName = "Pretty Privilege";
        this.description = "Facecam cells also give money for " + duration + " rounds.";
    }

    public override void OnAcquired()
    {
        GridManager.Instance.facecamGivesMoney = true;
    }

    public override void ApplyPowerUp()
    {
        GridManager.Instance.facecamGivesMoney = true;
    }

    public override void OnExpired()
    {
        GridManager.Instance.facecamGivesMoney = false;
    }
}
