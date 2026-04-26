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
    private int absoluteMileageGoal = 0;

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
    
    public void RegisterTickable(ITickable t)
    {
        if (t != null && !tickables.Contains(t))
            tickables.Add(t);
    }

    public void UnregisterTickable(ITickable t)
    {
        tickables.Remove(t);
    }

    private void BuildTickableList()
    {
        tickables.Clear();
        // if (bulletManager != null)        tickables.Add(bulletManager);
        if (enemyManager != null)         tickables.Add(enemyManager);
        if (enemyBulletManager != null)   tickables.Add(enemyBulletManager);
        if (mileageSystem != null)        tickables.Add(mileageSystem);
        if (defaultRangerContext != null) tickables.Add(defaultRangerContext);
        EnemySpawnSystem spawner = FindFirstObjectByType<EnemySpawnSystem>();
        if (spawner != null) tickables.Add(spawner);
    }

    // MODIFIED: BeginRun() — set absolute goal for level 1 too
    public void BeginRun()
    {
        isRunning     = true;
        shouldShoot   = true;
        goalTriggered = false;
        absoluteMileageGoal = sessionMileageOffset +
                              (CurrentLevel != null ? CurrentLevel.mileageGoal : 0);

        if (SoundManager.instance != null) SoundManager.instance.SetGameState();

        EnemySpawnSystem spawner = FindFirstObjectByType<EnemySpawnSystem>();
        if (spawner != null) spawner.ResetSpawner();

        ProceduralMapGenerator mapGen = FindFirstObjectByType<ProceduralMapGenerator>();
        if (mapGen != null && CurrentLevel != null)
        {
            string subfolder = !string.IsNullOrEmpty(CurrentLevel.mapSpritesSubfolder)
                ? CurrentLevel.mapSpritesSubfolder : "Level1";
            mapGen.BeginLevel(CurrentLevel.sqlLevelId > 0 ? CurrentLevel.sqlLevelId : 1, subfolder);
        }

        // load dlc skin
        LevelSkinApplier skinApplier = FindFirstObjectByType<LevelSkinApplier>();
        if (skinApplier != null && CurrentLevel != null)
            skinApplier.BeginLevel(CurrentLevel.dlcPackId);
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
        if (shouldShoot)
        {
            for (int i = tickables.Count - 1; i >= 0; i--)
            {
                if (i >= tickables.Count) continue;
                tickables[i]?.Tick(dt);
            }
            playerShooter.Tick(dt);
        }
        bulletManager.Tick(dt);
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
        int killed = enemyManager != null ? enemyManager.EnemiesKilled : 0;
        int traveled = mileageSystem != null ? mileageSystem.CurrentMiles : 0;
        LevelInfoSO level = CurrentLevel;
        int relativeProgress = traveled - sessionMileageOffset; // remaining is relative to current level only
        int levelGoal = level != null ? level.mileageGoal : 0; 
        int remaining = Mathf.Max(0, levelGoal - relativeProgress);
        int lost = playerLife != null ? playerLife.LivesLost : 0;
        return new RunStats { enemiesKilled = killed, mileageTraveled = traveled,
            mileageRemaining = remaining, livesLost = lost };
    }
    // MODIFIED: CheckMileageGoalReached() — use absoluteMileageGoal not levelGoal
    private void CheckMileageGoalReached()
    {
        if (goalTriggered) return;
        LevelInfoSO level = CurrentLevel;
        if (level == null || mileageSystem == null) return;
        // checking against absolute goals
        if (mileageSystem.CurrentMiles >= absoluteMileageGoal)
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
    
    private void ClearAllEnemies()
    {
        if (enemyManager != null) enemyManager.ClearAll();
        if (enemyBulletManager != null) enemyBulletManager.ClearAll();
    }

    private void SpawnBossIntro()
    {
        LevelInfoSO level = CurrentLevel;
        if (level == null || level.levelBoss == null || level.bossSpawnPos == null) return;
        GameObject bossObj = Instantiate(level.levelBoss);
        activeBoss = bossObj.GetComponent<LevelBoss>();
        if (activeBoss != null) activeBoss.Activate(level.bossSpawnPos.position);
        
        if (LevelSkinApplier.Instance != null)
            LevelSkinApplier.Instance.ApplySkinToBoss(bossObj);
        
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
        ClearAllEnemies();
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

        // absolute goal is where the player is currently (milleage wise) + this (new) level's goal
        absoluteMileageGoal = sessionMileageOffset +
                              (CurrentLevel != null ? CurrentLevel.mileageGoal : 0);
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