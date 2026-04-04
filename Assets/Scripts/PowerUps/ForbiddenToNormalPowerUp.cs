using UnityEngine;

public class ForbiddenToNormalPowerUp : PowerUp
{
    private float chance;

    public ForbiddenToNormalPowerUp(float jimmysCut, float chance = 0.1f) :base(PaymentMode.JimmysCut, jimmysCut, 0,  10000)
    {
        this.powerUpType = PowerUpType.AfterZoneSetup;
        this.chance = chance;
        this.displayName = "Zone Cleaner";
        this.description = $"{chance * 100:0}% chance to clear each forbidden tile every round.";
    }

    public override void ApplyPowerUp()
    {
        GridManager gridManager = GridManager.Instance;
        for (int x = 0; x < gridManager.width; x++)
        {
            for (int y = 0; y < gridManager.height; y++)
            {
                if (gridManager.Grid[x, y].TileType == TileType.Forbidden && Random.value < chance)
                {
                    gridManager.Grid[x, y].TileType = TileType.Normal;
                }
            }
        }
    }
}
