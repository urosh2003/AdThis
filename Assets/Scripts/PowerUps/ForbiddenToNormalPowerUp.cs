using UnityEngine;

public class ForbiddenToNormalPowerUp : IPowerUp
{
    private float chance;

    public ForbiddenToNormalPowerUp(float jimmysCut, float chance = 0.1f) : base(jimmysCut)
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
                if (gridManager.Grid[x, y].TileType == TileType.Forbidden && Random.value < chance)
                {
                    gridManager.Grid[x, y].TileType = TileType.Normal;
                }
            }
        }
    }
}
