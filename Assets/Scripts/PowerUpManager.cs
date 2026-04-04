using System.Collections.Generic;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    public List<PowerUp> availablePowerUps;
    public List<PowerUp> currentPowerUps;

    public static PowerUpManager instance;

    void Awake()
    {
        instance = this;
        this.currentPowerUps = new List<PowerUp>();
        this.currentPowerUps.Add(new BottomGreenRowPowerUp(0.2f));
        this.currentPowerUps.Add(new FixedMoneyPerAdCellPowerUp(0.2f, 1000));
        this.currentPowerUps.Add(new BetterGreenMultiplierPowerUp(0.2f));
    }

    public bool TryPurchase(PowerUp powerUp)
    {
        if (powerUp.paymentMode == PaymentMode.MoneyCost)
        {
            if (GridManager.Instance.CurrentMoney < powerUp.moneyCost)
                return false;
            GridManager.Instance.CurrentMoney -= powerUp.moneyCost;
        }

        currentPowerUps.Add(powerUp);
        powerUp.OnAcquired();

        if (powerUp.IsInstant)
        {
            powerUp.ApplyPowerUp();
            currentPowerUps.Remove(powerUp);
        }

        return true;
    }

    public void TickRound()
    {
        foreach (var pu in currentPowerUps)
            pu.OnRoundTick();

        var expired = new List<PowerUp>();
        foreach (var pu in currentPowerUps)
        {
            if (pu.totalDuration > 0)
            {
                pu.remainingRounds--;
                if (pu.remainingRounds <= 0)
                    expired.Add(pu);
            }
        }

        foreach (var pu in expired)
        {
            pu.OnExpired();
            currentPowerUps.Remove(pu);
        }
    }

    public float GetTotalJimmysCut()
    {
        float totalJimmysCut = 0.0f;
        foreach (PowerUp powerUp in currentPowerUps)
            if (powerUp.paymentMode == PaymentMode.JimmysCut)
                totalJimmysCut += powerUp.jimmysCut;

        return totalJimmysCut;
    }

    public void ApplyPowerUps(PowerUpType powerUpType)
    {
        foreach (var powerUp in currentPowerUps)
        {
            if (powerUp.powerUpType == powerUpType && !powerUp.IsExpired)
            {
                powerUp.ApplyPowerUp();
            }
        }
    }

    public bool HasActivePowerUp<T>() where T : PowerUp
    {
        foreach (var pu in currentPowerUps)
            if (pu is T && !pu.IsExpired)
                return true;
        return false;
    }

    public T GetActivePowerUp<T>() where T : PowerUp
    {
        foreach (var pu in currentPowerUps)
            if (pu is T && !pu.IsExpired)
                return (T)pu;
        return null;
    }
}
