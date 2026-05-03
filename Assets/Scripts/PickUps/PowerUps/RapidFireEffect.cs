
public class RapidFireEffect : PowerUpEffect
{
    private readonly float multiplier;

    public RapidFireEffect(float multiplier = 2.5f)
    {
        this.multiplier = multiplier;
    }

    public override void Apply()
    {
        shooter?.ApplyOverride(multiplier * GetBaseShotsPerSecond(), -1, -1f, null);
    }

    public override void Remove()
    {
        shooter?.ClearOverride();
    }
    
    private float GetBaseShotsPerSecond()
    {
        return shooter != null ? shooter.BaseShotsPerSecond : 10f;
    }
}
