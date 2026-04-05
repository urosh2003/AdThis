using System;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    public List<PowerUp> availablePowerUps;
    public List<PowerUp> currentPowerUps;

    [SerializeField] private PowerUpSelectionUI selectionUI;

    public static PowerUpManager instance;

    void Awake()
    {
        instance = this;
        this.currentPowerUps = new List<PowerUp>();
        availablePowerUps = new List<PowerUp>
        {
            new BottomGreenRowPowerUp(0.12f),
            new FixedMoneyPerAdCellPowerUp(0.12f, 1000),
            new BetterGreenMultiplierPowerUp(0.12f),
            new ForbiddenToNormalPowerUp(0.12f),
            new NormalToBonusPowerUp(0.12f),
            new AdBreakPowerUp(),
            new AntiAdBlockerPowerUp(),
            new ApologyVideoPowerUp(),
            new CancelablePowerUp(),
            new ColabPowerUp(),
            new MultiLevelMarketingPowerUp(),
            new PrettyPrivilegePowerUp(),
            new PrimetimeSlotPowerUp(),
            new ThumbnailTailorPowerUp(),
            new UncancelablePowerUp()
        };
    }

    public void Purchase(PowerUp powerUp)
    {
        if (powerUp.paymentMode == PaymentMode.MoneyCost)
        {
            int currentMoney = GridManager.Instance.CurrentMoney;
            if (currentMoney < powerUp.moneyCost)
            {
                float coefficient = (float)currentMoney / powerUp.moneyCost;
                powerUp.Scale(coefficient);
            }
            GridManager.Instance.CurrentMoney -= powerUp.moneyCost;
        }

        currentPowerUps.Add(powerUp);
        powerUp.OnAcquired();

        if (powerUp.IsInstant)
        {
            powerUp.ApplyPowerUp();
            currentPowerUps.Remove(powerUp);
        }
    }

    public void TryOfferPowerUpSelection(Action onComplete)
    {
        if (availablePowerUps.Count == 0 || RoundManager.Instance.roundNumber % 2 != 0)
        {
            onComplete?.Invoke();
            return;
        }

        List<PowerUp> choices = GetRandomChoices(2);

        if (choices.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        if (selectionUI == null)
        {
            onComplete?.Invoke();
            return;
        }

        selectionUI.Show(choices, selected =>
        {
            if (selected != null)
            {
                Purchase(selected);
                availablePowerUps.Remove(selected);
            }
            onComplete?.Invoke();
        });
    }
    
    private List<PowerUp> GetRandomChoices(int count)
    {
        var pool = new List<PowerUp>(availablePowerUps);
        var choices = new List<PowerUp>();

        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int idx = UnityEngine.Random.Range(0, pool.Count);
            choices.Add(pool[idx]);
            pool.RemoveAt(idx);
        }
        return choices;
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
