using UnityEngine;

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
        if (shield != null) shield.Deactivate();
    }
}
