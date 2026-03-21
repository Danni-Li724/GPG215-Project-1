using UnityEngine;

public class BobClasher : BaseClasher, IEnemyActivatable
{
    private Bob bob; 
    public override void Activate(Vector2 position, Transform playerTarget, EnemyPool ownerPool)
    {
        base.Activate(position, playerTarget, ownerPool);
        bob = GetComponent<Bob>();
    }
    
    public override void Tick(float dt)
    {
        base.Tick(dt);
        if(bob!=null) bob.DoBob();
    }
}
