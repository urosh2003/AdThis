public class BottomGreenRowPowerUp : PowerUp
{
    public BottomGreenRowPowerUp(float jimmysCut)
        : base(PaymentMode.JimmysCut, jimmysCut, 0, -1)
    {
        this.powerUpType = PowerUpType.AfterZoneSetup;
        this.displayName = "Bottom Green Row";
        this.description = "Converts the bottom row to bonus tiles every round.";
    }

    public override void ApplyPowerUp()
    {
        GridManager gridManager = GridManager.Instance;
        for (int i = 0; i < gridManager.width; i++)
        {
            gridManager.Grid[i, 0].TileType = TileType.Bonus;
        }
    }
}
