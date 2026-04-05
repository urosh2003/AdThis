public class ThumbnailTailorPowerUp : PowerUp
{
    private int viewerBonus;

    public ThumbnailTailorPowerUp(int moneyCost = 75000, int viewerBonus = 8000)
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

    public override void Scale(float coefficient)
    {
        base.Scale(coefficient);
        viewerBonus = (int)(viewerBonus * coefficient);
        this.description = "Instantly gain " + viewerBonus + " viewers.";
    }
}
