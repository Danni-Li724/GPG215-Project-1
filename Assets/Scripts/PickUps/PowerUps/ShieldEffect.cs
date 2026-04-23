using UnityEngine;

// Activates a child ShieldHealth GameObject on the player.
// The shield has its own collider + ShieldHealth component (IDamageable)
// that absorbs IDanger hits independently of PlayerLifeSystem.
public class ShieldEffect : PowerUpEffect
{
    private readonly ShieldHealth shield;

    public ShieldEffect(ShieldHealth shield)
    {
        this.shield = shield;
    }

    public override void Apply()
    {
        if (shield != null) shield.Activate();
    }

    public override void Remove()
    {
        // shield may have already broken on its own — safe either way
        if (shield != null) shield.Deactivate();
    }
}
