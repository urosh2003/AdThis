using System.Collections.Generic;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    public List<IPowerUp> availablePowerUps;
    public List<IPowerUp> currentPowerUps;

    
    public static PowerUpManager instance;

    void Awake()
    {
        instance = this;
        this.currentPowerUps = new List<IPowerUp>();
        this.currentPowerUps.Add(new BottomGreenRowPowerUp(0.2f));
        this.currentPowerUps.Add(new FixedMoneyPerAdCellPowerUp(0.2f, 1000));
        this.currentPowerUps.Add(new BetterGreenMultiplierPowerUp(0.2f));

    }

    public float GetTotalJimmysCut()
    {
        float totalJimmysCut = 0.0f;
        foreach (IPowerUp powerUp in currentPowerUps)
            totalJimmysCut += powerUp.jimmysCut;
        
        return totalJimmysCut;
    }
    
    public void ApplyPowerUps(PowerUpType powerUpType)
    {
        foreach (var powerUp in currentPowerUps)
        {
            if (powerUp.powerUpType == powerUpType)
            {
                powerUp.ApplyPowerUp();
            }
        }
        
    }
}
