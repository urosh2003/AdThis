using System;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    public List<IPowerUp> availablePowerUps;
    public List<IPowerUp> currentPowerUps;

    [SerializeField] private PowerUpSelectionUI selectionUI;

    public static PowerUpManager instance;

    void Awake()
    {
        instance = this;
        currentPowerUps = new List<IPowerUp>();

        availablePowerUps = new List<IPowerUp>
        {
            new BottomGreenRowPowerUp(0.2f),
            new FixedMoneyPerAdCellPowerUp(0.2f, 1000),
            new BetterGreenMultiplierPowerUp(0.2f),
            new ForbiddenToNormalPowerUp(0.15f),
            new NormalToBonusPowerUp(0.15f)
        };

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

    public void TryOfferPowerUpSelection(Action onComplete)
    {
        if (availablePowerUps.Count == 0 || RoundManager.Instance.roundNumber % 2 != 0)
        {
            onComplete?.Invoke();
            return;
        }

        List<IPowerUp> choices = GetRandomChoices(2);

        if (choices.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        selectionUI.Show(choices, selected =>
        {
            if (selected != null)
            {
                currentPowerUps.Add(selected);
                availablePowerUps.Remove(selected);
            }
            onComplete?.Invoke();
        });
    }

    private List<IPowerUp> GetRandomChoices(int count)
    {
        var pool = new List<IPowerUp>(availablePowerUps);
        var choices = new List<IPowerUp>();

        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int idx = UnityEngine.Random.Range(0, pool.Count);
            choices.Add(pool[idx]);
            pool.RemoveAt(idx);
        }
        return choices;
    }
}
