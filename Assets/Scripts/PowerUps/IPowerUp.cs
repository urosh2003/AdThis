public enum PowerUpType { AfterZoneSetup, DuringScoring, AfterScoring }

public abstract class IPowerUp
{
    public float jimmysCut;
    public PowerUpType powerUpType;
    public string displayName;
    public string description;

    public IPowerUp(float jimmysCut, string displayName, string description)
    {
        this.jimmysCut = jimmysCut;
        this.displayName = displayName;
        this.description = description;
    }

    public abstract void ApplyPowerUp();
}
