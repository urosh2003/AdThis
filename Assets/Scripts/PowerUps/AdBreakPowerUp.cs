public class AdBreakPowerUp : PowerUp
{
    public AdBreakPowerUp(int moneyCost = 200000)
        : base(PaymentMode.MoneyCost, 0f, moneyCost, 1)
    {
        this.powerUpType = PowerUpType.AfterZoneSetup;
        this.displayName = "Ad Break";
        this.description = "All tiles become green (bonus) for 1 round.";
    }

    public override void ApplyPowerUp()
    {
        GridManager gridManager = GridManager.Instance;
        for (int x = 0; x < gridManager.width; x++)
        {
            for (int y = 0; y < gridManager.height; y++)
            {
                gridManager.Grid[x, y].TileType = TileType.Bonus;
            }
        }
    }
}
