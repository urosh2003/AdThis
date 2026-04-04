public class ColabPowerUp : PowerUp
{
    private int viewersPerRound;

    public ColabPowerUp(float jimmysCut = 0.1f, int viewersPerRound = 30, int duration = 4)
        : base(PaymentMode.JimmysCut, jimmysCut, 0, duration)
    {
        this.powerUpType = PowerUpType.OnRoundStart;
        this.viewersPerRound = viewersPerRound;
        this.displayName = "Colab";
        this.description = "Gain " + viewersPerRound + " viewers each round for " + duration + " rounds.";
    }

    public override void ApplyPowerUp()
    {
        GridManager.Instance.CurrentViewers += viewersPerRound;
    }
}
