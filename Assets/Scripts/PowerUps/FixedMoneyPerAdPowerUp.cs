public class BottomGreenRowPowerUp : IPowerUp
{
    public int moneyPerAd;
    
    public BottomGreenRowPowerUp(float jimmysCut, int moneyPerAd) : base(jimmysCut)
    {
        this.powerUpType = PowerUpType.DuringScoring;
        this.moneyPerAd = moneyPerAd;
    }


    public override void ApplyPowerUp()
    {
        GridManager gridManager = GridManager.Instance;
        gridManager.moneyPerAd = moneyPerAd;
    }
}
