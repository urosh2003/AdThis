using UnityEngine;

public class MultiLevelMarketingPowerUp : PowerUp
{
    private int baseViewers;
    private int multiplier;
    private int currentInternalRound;

    public MultiLevelMarketingPowerUp(float jimmysCut = 0.12f, int baseViewers = 1000, int multiplier = 2, int duration = 4)
        : base(PaymentMode.JimmysCut, jimmysCut, 0, duration)
    {
        this.powerUpType = PowerUpType.OnRoundStart;
        this.baseViewers = baseViewers;
        this.multiplier = multiplier;
        this.currentInternalRound = -1;
        this.displayName = "Multi-Level Marketing";
        var rounds = new string[duration];
        for (int i = 0; i < duration; i++)
            rounds[i] = (baseViewers * (int)Mathf.Pow(multiplier, i)).ToString();
        this.description = "Gain " + string.Join(", ", rounds) + " viewers over " + duration + " rounds.";
    }

    public override void ApplyPowerUp()
    {
        int viewers = baseViewers * (int)Mathf.Pow(multiplier, currentInternalRound);
        GridManager.Instance.CurrentViewers += viewers;
    }

    public override void OnRoundTick()
    {
        currentInternalRound++;
    }
}
