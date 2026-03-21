using UnityEngine;

public interface IEnemyActivatable
{
    void Activate(Vector2 position, Transform playerTarget, EnemyPool ownerPool);
}