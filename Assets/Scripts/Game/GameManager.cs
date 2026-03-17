// GameManager.cs
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
    [SerializeField] private int currentLevelIndex = 0;

    private bool goalTriggered;
    private bool bossSpawned;
    private bool destinationSpawned;

    private LevelBoss activeBoss;
    private GameObject activeDestination;

    private bool isLerpingBoss;
    private bool isLerpingDestination;

    [Header("UIs")]
    public LevelInfoPanelUI levelInfoPanel;
    public GameOverPanelUI gameOverPanel;
    public HUDUI hudUI;

    public bool isRunning;
    public bool shouldShoot;
    
    public struct RunStats
    {
        public int enemiesKilled;
        public int mileageTraveled;
        public int mileageRemaining;
        public int livesLost;
    }

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
        isRunning = false;

        if (levelInfoPanel != null && CurrentLevel != null)
            levelInfoPanel.Show(CurrentLevel);
        if (hudUI != null)
            hudUI.BindPanels(gameOverPanel);
    }

    private void BuildTickableList()
    {
        tickables.Clear();
        // if (playerShooter != null) tickables.Add(playerShooter);
        if (bulletManager != null) tickables.Add(bulletManager);
        if (enemyManager != null) tickables.Add(enemyManager);
        if (enemyBulletManager != null) tickables.Add(enemyBulletManager);
        if (mileageSystem != null) tickables.Add(mileageSystem);
        if (defaultRangerContext != null) tickables.Add(defaultRangerContext);
    }

    public void BeginRun()
    {
        isRunning = true;
        shouldShoot = true;
        SoundManager.instance.SetGameState();
    }

    private void Update()
    {
        if (!isRunning)
            return;

        float dt = Time.deltaTime;

        for (int i = 0; i < tickables.Count; i++)
            tickables[i].Tick(dt);
        
        if (isRunning && shouldShoot) playerShooter.Tick(dt);
        CheckMileageGoalReached();
        TickBossAndDestination(dt);
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
    private RunStats BuildRunStats()
    {
        int killed = (enemyManager != null) ? enemyManager.EnemiesKilled : 0;
        int traveled = (mileageSystem != null) ? mileageSystem.CurrentMiles : 0;

        LevelInfoSO level = CurrentLevel;
        int goal = (level != null) ? level.mileageGoal : 0;
        int remaining = Mathf.Max(0, goal - traveled);

        int livesLost = (playerLife != null) ? playerLife.LivesLost : 0;

        return new RunStats
        {
            enemiesKilled = killed,
            mileageTraveled = traveled,
            mileageRemaining = remaining,
            livesLost = livesLost
        };
    }

    private void CheckMileageGoalReached()
    {
        if (goalTriggered)
            return;

        LevelInfoSO level = CurrentLevel;
        if (level == null || mileageSystem == null)
            return;

        if (mileageSystem.CurrentMiles >= level.mileageGoal)
        {
            goalTriggered = true;
            shouldShoot = false;

            if (hudUI != null)
            {
                string notice = (!string.IsNullOrWhiteSpace(level.bossNoticeText))
                    ? level.bossNoticeText
                    : "Boss Incoming!";

                float hold = (level.bossNoticeDuration > 0f) ? level.bossNoticeDuration : 3f;
                hudUI.PlayBossNotice(notice, hold, () =>
                {
                    SpawnBossIntro();
                    isRunning = true;
                });
            }
            else
            {
                SpawnBossIntro();
                isRunning = true;
            }
        }
    }

    private void SpawnBossIntro()
    {
        LevelInfoSO level = CurrentLevel;
        if (level == null || level.levelBoss == null || level.bossSpawnPos == null)
            return;

        GameObject bossObj = Instantiate(level.levelBoss);
        activeBoss = bossObj.GetComponent<LevelBoss>();
        if (activeBoss != null)
            activeBoss.Activate(level.bossSpawnPos.position);

        bossSpawned = true;
        isLerpingBoss = true;
        shouldShoot = true;
    }

    private void TickBossAndDestination(float dt)
    {
        LevelInfoSO level = CurrentLevel;
        if (level == null)
            return;

        if (isLerpingBoss && activeBoss != null && level.bossFinalPos != null)
        {
            Vector3 pos = activeBoss.transform.position;
            Vector3 target = level.bossFinalPos.position;

            activeBoss.transform.position = Vector3.MoveTowards(pos, target, level.bossMoveSpeed * dt);

            if (Vector3.SqrMagnitude(activeBoss.transform.position - target) < 0.0001f)
                isLerpingBoss = false;
        }

        if (isLerpingDestination && activeDestination != null && level.destinationFinalPos != null)
        {
            Vector3 pos = activeDestination.transform.position;
            Vector3 target = level.destinationFinalPos.position;

            activeDestination.transform.position = Vector3.MoveTowards(pos, target, level.destinationMoveSpeed * dt);

            if (Vector3.SqrMagnitude(activeDestination.transform.position - target) < 0.0001f)
            {
                isLerpingDestination = false;
                OnDestinationArrived();
            }
        }
    }

    public void OnBossKilled()
    {
        SpawnDestinationIntro();
    }

    private void SpawnDestinationIntro()
    {
        LevelInfoSO level = CurrentLevel;
        if (level == null || level.levelDestination == null || level.destinationSpawnPos == null)
            return;

        activeDestination = Instantiate(level.levelDestination);
        activeDestination.transform.position = level.destinationSpawnPos.position;

        destinationSpawned = true;
        isLerpingDestination = true;
    }

    private void OnDestinationArrived()
    {
        shouldShoot = false;
        LevelInfoSO level = CurrentLevel;
        
        string msg;
        if (level != null && !string.IsNullOrWhiteSpace(level.arrivalNoticeText))
            msg = level.arrivalNoticeText;
        else if (level != null)
            msg = "You have now arrived at " + level.levelName;
        else
            msg = "You have arrived!";

        float hold = (level != null && level.arrivalNoticeDuration > 0f) ? level.arrivalNoticeDuration : 2f;

        if (hudUI != null)
        {
            hudUI.PlayArrivalNotice(msg, hold, () =>
            {
                RunStats s = BuildRunStats();
                hudUI.ShowProceedToNextLevelPanel(s);
            });
        }
        else
        {
        
            RunStats s = BuildRunStats();
            if (hudUI != null)
                hudUI.ShowProceedToNextLevelPanel(s);
        }
    }
    
    public void GameOver(int livesLost)
    {
        isRunning = false;

        RunStats s = BuildRunStats();
        s.livesLost = livesLost; 

        if (hudUI != null)
            hudUI.ShowGameOverPanel(s);
    }
    public void StartNextLevel()
    {
        GoToNextLevel();

        ResetLevel();

        isRunning = false;

        if (levelInfoPanel != null && CurrentLevel != null)
            levelInfoPanel.Show(CurrentLevel);
    }
    public void RestartCurrentLevel()
    {
        ResetLevel();

        isRunning = false;

        if (levelInfoPanel != null && CurrentLevel != null)
            levelInfoPanel.Show(CurrentLevel);
    }

    private void GoToNextLevel()
    {
        if (levels == null || levels.Count == 0)
            return;

        currentLevelIndex += 1;

        if (currentLevelIndex >= levels.Count)
            currentLevelIndex = levels.Count - 1;
    }

    private void ResetLevel()
    {
        goalTriggered = false;
        bossSpawned = false;
        destinationSpawned = false;
        isLerpingBoss = false;
        isLerpingDestination = false;

        if (activeDestination != null)
        {
            Destroy(activeDestination);
            activeDestination = null;
        }

        if (activeBoss != null)
        {
            Destroy(activeBoss.gameObject);
            activeBoss = null;
        }
    }
}