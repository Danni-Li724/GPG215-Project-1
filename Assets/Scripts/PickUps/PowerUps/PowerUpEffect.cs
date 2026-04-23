using UnityEngine;

// Abstract base for all powerup effects. Subclasses implement Apply/Remove.
// PlayerPowerUpSystem owns the active effect and calls EndEffect() after duration.
public abstract class PowerUpEffect
{
    protected PlayerShooter shooter;
    protected float duration;

    public virtual void Init(PlayerShooter shooter, float duration)
    {
        this.shooter  = shooter;
        this.duration = duration;
    }

    public abstract void Apply();
    public abstract void Remove();
}
