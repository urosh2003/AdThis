public class BetterGreenMultiplierPowerUp : IPowerUp
{
    public float newMultiplier;
    
    public BetterGreenMultiplierPowerUp(float jimmysCut, float newMultiplier=3f) : base(jimmysCut)
    {
        this.powerUpType = PowerUpType.DuringScoring;
        this.newMultiplier = newMultiplier;
    }


    public override void ApplyPowerUp()
    {
        GridManager gridManager = GridManager.Instance;
        gridManager.bonusMultiplier = newMultiplier;
    }
}
