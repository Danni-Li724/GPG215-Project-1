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

    private int sessionMileageOffset = 0;

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
            Destroy(instance.gameObject); 
        }
        instance = this;

        BuildTickableList();
        isRunning   = false;
        shouldShoot = false;

        if (levelInfoPanel != null && CurrentLevel != null)
            levelInfoPanel.Show(CurrentLevel);
        if (hudUI != null)
            hudUI.BindPanels(gameOverPanel);
    }

    private void BuildTickableList()
    {
        tickables.Clear();
        if (bulletManager != null)        tickables.Add(bulletManager);
        if (enemyManager != null)         tickables.Add(enemyManager);
        if (enemyBulletManager != null)   tickables.Add(enemyBulletManager);
        if (mileageSystem != null)        tickables.Add(mileageSystem);
        if (defaultRangerContext != null) tickables.Add(defaultRangerContext);
        EnemySpawnSystem spawner = FindFirstObjectByType<EnemySpawnSystem>();
        if (spawner != null) tickables.Add(spawner);
    }

    public void BeginRun()
    {
        isRunning   = true;
        shouldShoot = true;
        goalTriggered = false;
        SoundManager.instance.SetGameState();
        EnemySpawnSystem spawner = FindFirstObjectByType<EnemySpawnSystem>();
        if (spawner != null) spawner.ResetSpawner();
        
        ProceduralMapGenerator mapGen = FindFirstObjectByType<ProceduralMapGenerator>();
        if (mapGen != null && CurrentLevel != null)
            mapGen.BeginLevel(CurrentLevel.sqlLevelId > 0 ? CurrentLevel.sqlLevelId : 1);
    }

    public void ResumeRun()
    {
        isRunning   = true;
        shouldShoot = true;
    }

    private void Update()
    {
        if (!isRunning) return;
        float dt = Time.deltaTime;
        for (int i = 0; i < tickables.Count; i++) tickables[i].Tick(dt);
        if (shouldShoot) playerShooter.Tick(dt);
        CheckMileageGoalReached();
        TickBossAndDestination(dt);
    }

    public LevelInfoSO CurrentLevel
    {
        get
        {
            if (levels == null || levels.Count == 0) return null;
            currentLevelIndex = Mathf.Clamp(currentLevelIndex, 0, levels.Count - 1);
            return levels[currentLevelIndex];
        }
    }

    private RunStats BuildRunStats()
    {
        int killed   = enemyManager != null ? enemyManager.EnemiesKilled : 0;
        int traveled = mileageSystem != null ? mileageSystem.CurrentMiles : 0;
        LevelInfoSO level = CurrentLevel;
        int goal      = level != null ? level.mileageGoal : 0;
        int remaining = Mathf.Max(0, goal - traveled);
        int lost      = playerLife != null ? playerLife.LivesLost : 0;
        return new RunStats { enemiesKilled = killed, mileageTraveled = traveled,
                              mileageRemaining = remaining, livesLost = lost };
    }

    private void CheckMileageGoalReached()
    {
        if (goalTriggered) return;
        LevelInfoSO level = CurrentLevel;
        if (level == null || mileageSystem == null) return;
        if (mileageSystem.CurrentMiles >= level.mileageGoal)
        {
            goalTriggered = true;
            shouldShoot   = false;
            string notice = !string.IsNullOrWhiteSpace(level.bossNoticeText)
                ? level.bossNoticeText : "Boss Incoming!";
            float hold = level.bossNoticeDuration > 0f ? level.bossNoticeDuration : 3f;
            if (hudUI != null)
                hudUI.PlayBossNotice(notice, hold, () => { SpawnBossIntro(); isRunning = true; });
            else
            { SpawnBossIntro(); isRunning = true; }
        }
    }

    private void SpawnBossIntro()
    {
        LevelInfoSO level = CurrentLevel;
        if (level == null || level.levelBoss == null || level.bossSpawnPos == null) return;
        GameObject bossObj = Instantiate(level.levelBoss);
        activeBoss = bossObj.GetComponent<LevelBoss>();
        if (activeBoss != null) activeBoss.Activate(level.bossSpawnPos.position);
        bossSpawned   = true;
        isLerpingBoss = true;
        shouldShoot   = true;
    }

    private void TickBossAndDestination(float dt)
    {
        LevelInfoSO level = CurrentLevel;
        if (level == null) return;
        if (isLerpingBoss && activeBoss != null && level.bossFinalPos != null)
        {
            activeBoss.transform.position = Vector3.MoveTowards(
                activeBoss.transform.position, level.bossFinalPos.position, level.bossMoveSpeed * dt);
            if (Vector3.SqrMagnitude(activeBoss.transform.position - level.bossFinalPos.position) < 0.0001f)
                isLerpingBoss = false;
        }
        if (isLerpingDestination && activeDestination != null && level.destinationFinalPos != null)
        {
            activeDestination.transform.position = Vector3.MoveTowards(
                activeDestination.transform.position, level.destinationFinalPos.position, level.destinationMoveSpeed * dt);
            if (Vector3.SqrMagnitude(activeDestination.transform.position - level.destinationFinalPos.position) < 0.0001f)
            { isLerpingDestination = false; OnDestinationArrived(); }
        }
    }

    public void OnBossKilled() { SpawnDestinationIntro(); }

    private void SpawnDestinationIntro()
    {
        LevelInfoSO level = CurrentLevel;
        if (level == null || level.levelDestination == null || level.destinationSpawnPos == null) return;
        activeDestination = Instantiate(level.levelDestination);
        activeDestination.transform.position = level.destinationSpawnPos.position;
        destinationSpawned   = true;
        isLerpingDestination = true;
    }

    private void OnDestinationArrived()
    {
        shouldShoot = false;
        LevelInfoSO level = CurrentLevel;
        string msg = level != null && !string.IsNullOrWhiteSpace(level.arrivalNoticeText)
            ? level.arrivalNoticeText
            : (level != null ? "You have arrived at " + level.levelName : "You arrived!");
        float hold = level != null && level.arrivalNoticeDuration > 0f ? level.arrivalNoticeDuration : 2f;
        if (hudUI != null)
            hudUI.PlayArrivalNotice(msg, hold, () => hudUI.ShowProceedToNextLevelPanel(BuildRunStats()));
    }

    public void GameOver(int livesLost)
    {
        isRunning   = false;
        shouldShoot = false;
        RunStats s  = BuildRunStats();
        s.livesLost = livesLost;

        if (MirageSaveSystem.Instance != null)
        {
            RunResultData result = new RunResultData
            {
                totalMileage  = s.mileageTraveled,
                enemiesKilled = s.enemiesKilled,
                livesLost     = s.livesLost
            };
            bool isNewBest = MirageSaveSystem.Instance.TrySaveIfBest(result);
            if (isNewBest && hudUI != null) hudUI.ShowHighScoreNotice(s.mileageTraveled);
        }

        if (hudUI != null) hudUI.ShowGameOverPanel(s);
    }

    public void StartNextLevel()
    {
        sessionMileageOffset = mileageSystem != null ? mileageSystem.CurrentMiles : 0;
        GoToNextLevel();
        ResetLevel();
        if (mileageSystem != null) mileageSystem.SetStartMileage(sessionMileageOffset);
        if (levelInfoPanel != null && CurrentLevel != null) levelInfoPanel.Show(CurrentLevel);
    }

    public void RestartCurrentLevel()
    {
        sessionMileageOffset = 0;
        ResetLevel();
        if (levelInfoPanel != null && CurrentLevel != null) levelInfoPanel.Show(CurrentLevel);
    }

    private void GoToNextLevel()
    {
        if (levels == null || levels.Count == 0) return;
        currentLevelIndex = Mathf.Min(currentLevelIndex + 1, levels.Count - 1);
    }

    private void ResetLevel()
    {
        goalTriggered        = false;
        bossSpawned          = false;
        destinationSpawned   = false;
        isLerpingBoss        = false;
        isLerpingDestination = false;
        isRunning            = false;
        shouldShoot          = false;
        if (activeDestination != null) { Destroy(activeDestination); activeDestination = null; }
        if (activeBoss != null)        { Destroy(activeBoss.gameObject); activeBoss = null; }
    }
}