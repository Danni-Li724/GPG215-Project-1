// Overrides PlayerShooter to fire 3 bullets in a spread.
// Reads bullet count and spread from the inspector values baked into PlayerShooter's
// ApplyOverride — we just pass our desired values.
public class TriShotEffect : PowerUpEffect
{
    private readonly float spreadDegrees;

    public TriShotEffect(float spreadDegrees = 40f)
    {
        this.spreadDegrees = spreadDegrees;
    }

    public override void Apply()
    {
        shooter?.ApplyOverride(-1f, 3, spreadDegrees, null);
    }

    public override void Remove()
    {
        shooter?.ClearOverride();
    }
}
