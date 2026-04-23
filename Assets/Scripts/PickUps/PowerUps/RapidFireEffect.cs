// Doubles the player's fire rate for the effect duration.
public class RapidFireEffect : PowerUpEffect
{
    private readonly float multiplier;

    public RapidFireEffect(float multiplier = 2.5f)
    {
        this.multiplier = multiplier;
    }

    public override void Apply()
    {
        // read current base rate from shooter by applying a high rate override
        // we don't need the original value — ClearOverride() restores it
        shooter?.ApplyOverride(multiplier * GetBaseShotsPerSecond(), -1, -1f, null);
    }

    public override void Remove()
    {
        shooter?.ClearOverride();
    }

    // PlayerShooter exposes its base rate via a property added below
    private float GetBaseShotsPerSecond()
    {
        return shooter != null ? shooter.BaseShotsPerSecond : 10f;
    }
}
