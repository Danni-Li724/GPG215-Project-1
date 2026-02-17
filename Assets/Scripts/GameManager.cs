using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
   private static GameManager instance;
   public BulletManager bulletManager;
   public PlayerShooter playerShooter;
   public HitVFXPoolManager hitVFXPoolManager;
   public EnemyManager enemyManager;
   private readonly List<ITickable> tickables = new List<ITickable>(6);
   private void Awake()
   {
       if (instance != null && instance != this)
       {
           Destroy(gameObject);
           return;
       }
       instance = this;
       DontDestroyOnLoad(gameObject);
       BuildTickableList();
   }
   
   private void BuildTickableList()
   {
       tickables.Clear();

       if (playerShooter != null) tickables.Add(playerShooter);
       if (bulletManager != null) tickables.Add(bulletManager);
       if (enemyManager != null) tickables.Add(enemyManager);
   }


    void Update()
    {
        float dt = Time.deltaTime;
        for (int i = 0; i < tickables.Count; i++)
            tickables[i].Tick(dt);
    }
}
