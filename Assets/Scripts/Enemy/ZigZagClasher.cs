using UnityEngine;

public class ZigZagClasher : BaseClasher, IEnemyActivatable
{
    private ZigZag zigzag;

    public override void Activate(Vector2 position, Transform playerTarget, EnemyPool ownerPool)
    {
        base.Activate(position, playerTarget, ownerPool);
        zigzag = GetComponent<ZigZag>();
        if (zigzag != null) zigzag.ResetZigZag();
    }

    public override void Tick(float dt)
    {
        base.Tick(dt);
        if (zigzag != null) zigzag.DoZigZag(dt);
    }
}