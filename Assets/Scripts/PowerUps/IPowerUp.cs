public enum PowerUpType { AfterZoneSetup, DuringScoring, AfterScoring }

public abstract class IPowerUp
{
    public float jimmysCut;
    public PowerUpType powerUpType;

    public IPowerUp(float jimmysCut)
    {
        this.jimmysCut = jimmysCut;
    }

    public abstract void ApplyPowerUp();
}
