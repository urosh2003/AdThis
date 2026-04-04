public class FixedMoneyPerAdCellPowerUp : IPowerUp
{
    public int moneyPerAdPerCell;
    
    public FixedMoneyPerAdCellPowerUp(float jimmysCut, int moneyPerAd) : base(jimmysCut, "Flat Ad Revenue", "Each ad cell earns a fixed $1,000 bonus.")
    {
        this.powerUpType = PowerUpType.DuringScoring;
        this.moneyPerAdPerCell = moneyPerAd;
    }


    public override void ApplyPowerUp()
    {
        GridManager gridManager = GridManager.Instance;
        gridManager.moneyPerAdPerCell = moneyPerAdPerCell;
    }
}
