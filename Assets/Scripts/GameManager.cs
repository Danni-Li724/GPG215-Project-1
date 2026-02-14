using UnityEngine;

public class GameManager : MonoBehaviour
{
   private static GameManager instance;
   public BulletManager bulletManager;
   public PlayerShooter playerShooter;
    void Start()
    {
        if(instance == null) instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    void Update()
    {
        playerShooter.Tick(Time.deltaTime);
        bulletManager.Tick(Time.deltaTime);
    }
}
