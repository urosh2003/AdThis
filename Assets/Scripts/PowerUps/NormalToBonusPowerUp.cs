using UnityEngine;

public class NormalToBonusPowerUp : PowerUp
{
    private float chance;

    public NormalToBonusPowerUp(float jimmysCut, float chance = 0.1f) : base(PaymentMode.JimmysCut, jimmysCut, 0,  1000)
    {
        this.powerUpType = PowerUpType.AfterZoneSetup;
        this.chance = chance;
        this.displayName = "Lucky Tiles";
        this.description = "10% chance to upgrade each normal tile to bonus.";
    }

    public override void ApplyPowerUp()
    {
        GridManager gridManager = GridManager.Instance;
        for (int x = 0; x < gridManager.width; x++)
        {
            for (int y = 0; y < gridManager.height; y++)
            {
                if (gridManager.Grid[x, y].TileType == TileType.Normal && Random.value < chance)
                {
                    gridManager.Grid[x, y].TileType = TileType.Bonus;
                }
            }
        }
    }
}
