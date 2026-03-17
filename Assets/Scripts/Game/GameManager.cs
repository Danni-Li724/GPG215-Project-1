using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
   public static GameManager instance;
   public BulletManager bulletManager;
   public PlayerShooter playerShooter;
   public HitVFXPoolManager hitVFXPoolManager;
   public EnemyManager enemyManager;
   public EnemyBulletManager enemyBulletManager; 
   public MileageSystem mileageSystem;
   public DefaultRangerContext defaultRangerContext;
   private readonly List<ITickable> tickables = new List<ITickable>();
   public PlayerLifeSystem playerLife;
   
   [Header("UIs")]
   public LevelInfoSO levelInfo;
   public LevelInfoPanelUI levelInfoPanel;
   public GameOverPanelUI gameOverPanel;
   
   private bool isRunning;
   
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
       // SoundManager.instance.SetGameState();
       isRunning = false;
       if (levelInfoPanel != null && levelInfo != null)
           levelInfoPanel.Show(levelInfo);
   }
   
   private void BuildTickableList()
   {
       tickables.Clear();
       if (playerShooter != null) tickables.Add(playerShooter);
       if (bulletManager != null) tickables.Add(bulletManager);
       if (enemyManager != null) tickables.Add(enemyManager);
       if (enemyBulletManager != null) tickables.Add(enemyBulletManager);
       if (mileageSystem != null) tickables.Add(mileageSystem);
       if (defaultRangerContext != null) tickables.Add(defaultRangerContext);
       
   }
   public void BeginRun()
   {
       isRunning = true;
       SoundManager.instance.SetGameState();
   }

   private void Update()
   {
       if (!isRunning)
           return;
       float dt = Time.deltaTime;
       for (int i = 0; i < tickables.Count; i++)
           tickables[i].Tick(dt);
   }

   public void GameOver(int livesLost)
   {
       isRunning = false;

       int killed = (enemyManager != null) ? enemyManager.EnemiesKilled : 0;
       int traveled = (mileageSystem != null) ? mileageSystem.CurrentMiles : 0;
       int goal = (levelInfo != null) ? levelInfo.mileageGoal : 0;
       int remaining = Mathf.Max(0, goal - traveled);
       if (gameOverPanel != null)
           gameOverPanel.Show(killed, traveled, remaining, livesLost);
   }
}
