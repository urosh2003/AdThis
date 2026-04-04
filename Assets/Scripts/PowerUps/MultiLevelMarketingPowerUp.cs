using UnityEngine;

public class MultiLevelMarketingPowerUp : PowerUp
{
    private int baseViewers;
    private int multiplier;
    private int currentInternalRound;

    public MultiLevelMarketingPowerUp(float jimmysCut = 0.2f, int baseViewers = 100, int multiplier = 3, int duration = 5)
        : base(PaymentMode.JimmysCut, jimmysCut, 0, duration)
    {
        this.powerUpType = PowerUpType.OnRoundStart;
        this.baseViewers = baseViewers;
        this.multiplier = multiplier;
        this.currentInternalRound = -1;
        this.displayName = "Multi-Level Marketing";
        this.description = "Gain escalating viewers each round (x" + multiplier + ") for " + duration + " rounds.";
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
