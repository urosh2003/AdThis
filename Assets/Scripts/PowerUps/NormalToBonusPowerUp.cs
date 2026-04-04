using UnityEngine;

public class NormalToBonusPowerUp : IPowerUp
{
    private float chance;

    public NormalToBonusPowerUp(float jimmysCut, float chance = 0.1f) : base(jimmysCut, "Lucky Tiles", "10% chance to upgrade each normal tile to bonus.")
    {
        this.powerUpType = PowerUpType.AfterZoneSetup;
        this.chance = chance;
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
