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
   [SerializeField] private List<LevelInfoSO> levels = new List<LevelInfoSO>();
   
   [Header("UIs")]
   [SerializeField] private int currentLevelIndex = 0;
   public LevelInfoPanelUI levelInfoPanel;
   public GameOverPanelUI gameOverPanel;
   
   public bool isRunning;
   
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
       if (levelInfoPanel != null && CurrentLevel != null)
           levelInfoPanel.Show(CurrentLevel);
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
   
   public LevelInfoSO CurrentLevel
   {
       get
       {
           if (levels == null || levels.Count == 0)
               return null;

           currentLevelIndex = Mathf.Clamp(currentLevelIndex, 0, levels.Count - 1);
           return levels[currentLevelIndex];
       }
   }

   public void GameOver(int livesLost)
   {
       isRunning = false;

       int killed = (enemyManager != null) ? enemyManager.EnemiesKilled : 0;
       int traveled = (mileageSystem != null) ? mileageSystem.CurrentMiles : 0;
       LevelInfoSO level = CurrentLevel;
       int goal = (level != null) ? level.mileageGoal : 0;
       int remaining = Mathf.Max(0, goal - traveled);
       if (gameOverPanel != null)
           gameOverPanel.Show(killed, traveled, remaining, livesLost);
   }
   
   public void GoToNextLevel()
   {
       if (levels == null || levels.Count == 0)
           return;
       currentLevelIndex += 1;
       if (currentLevelIndex >= levels.Count)
           currentLevelIndex = levels.Count - 1; 
   }
   
   public void StartNextLevel()
   {
       GoToNextLevel();
       // show next level panel again
       isRunning = false;
       if (levelInfoPanel != null && CurrentLevel != null)
           levelInfoPanel.Show(CurrentLevel);
   }
}
