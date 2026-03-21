using UnityEngine;

public class BoomerangClasher : BaseClasher, IEnemyActivatable
{
    private Rotate rotate; 
    
    public override void Activate(Vector2 position, Transform playerTarget, EnemyPool ownerPool)
    {
        base.Activate(position, playerTarget, ownerPool);
        rotate = GetComponent<Rotate>();
    }
    
    public override void Tick(float dt)
    {
        base.Tick(dt);
        if(rotate!=null) rotate.DoRotate();
    }
}
