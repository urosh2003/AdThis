using UnityEngine;

public enum PowerUpType { AfterZoneSetup, DuringScoring, AfterScoring, OnRoundStart }
public enum PaymentMode { JimmysCut, MoneyCost }

public abstract class PowerUp
{
    public string displayName;
    public string description;

    // Payment
    public PaymentMode paymentMode;
    public float jimmysCut;
    public int moneyCost;

    // Duration: -1 = permanent, 0 = instant, >0 = N rounds
    public int totalDuration;
    public int remainingRounds;

    // Timing
    public PowerUpType powerUpType;

    public bool IsExpired => totalDuration > 0 && remainingRounds <= 0;
    public bool IsPermanent => totalDuration == -1;
    public bool IsInstant => totalDuration == 0;

    public PowerUp(PaymentMode paymentMode, float jimmysCut, int moneyCost, int duration)
    {
        this.paymentMode = paymentMode;
        this.jimmysCut = paymentMode == PaymentMode.JimmysCut
            ? jimmysCut * Random.Range(0.75f, 1.25f)
            : 0f;
        this.moneyCost = paymentMode == PaymentMode.MoneyCost ? moneyCost : 0;
        this.totalDuration = duration;
        this.remainingRounds = duration;
    }

    /// Called when the power-up is first acquired.
    public virtual void OnAcquired() { }

    /// Called each round at the appropriate PowerUpType timing.
    public abstract void ApplyPowerUp();

    /// Called when duration expires or power-up is removed. Use to revert state.
    public virtual void OnExpired() { }

    /// Called once per round for internal state updates (before remainingRounds is decremented).
    public virtual void OnRoundTick() { }
}
