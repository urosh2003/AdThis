public class BottomGreenRowPowerUp : IPowerUp
{
    public BottomGreenRowPowerUp(float jimmysCut) : base(jimmysCut, "Green Bottom Row", "Converts the entire bottom row to bonus tiles.")
    {
        this.powerUpType = PowerUpType.AfterZoneSetup;
    }


    public override void ApplyPowerUp()
    {
        GridManager gridManager = GridManager.Instance;
        for(int i = 0; i < gridManager.width; i++)
        {
            gridManager.Grid[i, 0].TileType = TileType.Bonus;
        }
    }
}
