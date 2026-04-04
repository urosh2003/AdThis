public class BottomGreenRowPowerUp : IPowerUp
{
    public BottomGreenRowPowerUp(float jimmysCut) : base(jimmysCut)
    {
        this.powerUpType = PowerUpType.AfterZoneSetup;
    }


    public override void ApplyPowerUp()
    {
        GridManager gridManager = GridManager.Instance;
        for(int i = 0; i < gridManager.width; i++)
        {
            gridManager.Grid[0, i].TileType = TileType.Bonus;
        }
    }
}
