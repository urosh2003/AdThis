public class ThumbnailTailorPowerUp : PowerUp
{
    private int viewerBonus;

    public ThumbnailTailorPowerUp(int moneyCost = 30000, int viewerBonus = 50)
        : base(PaymentMode.MoneyCost, 0f, moneyCost, 0)
    {
        this.powerUpType = PowerUpType.OnRoundStart;
        this.viewerBonus = viewerBonus;
        this.displayName = "Thumbnail Tailor";
        this.description = "Instantly gain " + viewerBonus + " viewers.";
    }

    public override void ApplyPowerUp()
    {
        GridManager.Instance.CurrentViewers += viewerBonus;
    }
}
