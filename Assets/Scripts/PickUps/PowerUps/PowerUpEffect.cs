using UnityEngine;

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
