public class ApologyVideoPowerUp : PowerUp
{
    private int viewerBonus;
    private int moneyBonus;

    public ApologyVideoPowerUp(int moneyCost = 150000, int viewerBonus = 4000, int moneyBonus = 30000)
        : base(PaymentMode.MoneyCost, 0f, moneyCost, 0)
    {
        this.powerUpType = PowerUpType.OnRoundStart;
        this.viewerBonus = viewerBonus;
        this.moneyBonus = moneyBonus;
        this.displayName = "Apology Video";
        this.description = "Instantly gain " + viewerBonus + " viewers and $" + moneyBonus + ".";
    }

    public override void ApplyPowerUp()
    {
        GridManager.Instance.CurrentViewers += viewerBonus;
        GridManager.Instance.CurrentMoney += moneyBonus;
    }
}
