using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class DefaultRangerContext : MonoBehaviour, ITickable, IDamageable, IHitVFXGetter, IPoolableEnemy, IDanger, IEnemyActivatable
{
    [Header("States")]
    private State surveyState;
    private State moveState;
    private State shootState;
    private State lungeState;
    private State currentState;
    private State pendingState;
    public float MoveSpeed => moveSpeed; 
    public State MoveState => moveState;
    public State ShootState => shootState;
    private bool initialized;
    
    [SerializeField] private Transform player;
    [FormerlySerializedAs("bulletManager")] [SerializeField] private EnemyBulletManager enemyBulletManager;
    [SerializeField] private BulletTypeSO enemyBulletType;
    [SerializeField] private Transform firePoint;
    [SerializeField] private PolygonCollider2D moveArea;
    
    [Header("Survey")]
    [SerializeField] private float surveySeconds = 1.0f;
    private float surveyTimer;
    
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float lungeSpeed = 6.0f;
    [SerializeField] private float arriveDistance = 0.15f;

    [Header("Shoot Point")]
    [SerializeField] private float minDistanceFromLastPoint = 10f;
    [SerializeField] private int maxPickAttempts = 20;
    [SerializeField] private List<Vector2> shootPoints;
    private Vector2 lastPickedPoint;
    private bool hasLastPickedPoint;
    private float dt;
    private Vector2 targetPoint;
    private int cyclesCompleted;
    private int shotsFired;
    private float shotTimer;

    [Header("Shoot Pattern")]
    [SerializeField] private int shootPointCycles = 3;
    [SerializeField] private int bulletsPerBurst = 5;
    [SerializeField] private float timeBetweenShots = 0.12f;
    
    
    [Header("Interfaces")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private HitVFXType hitVFXType = HitVFXType.Default;
    public HitVFXType HitVFXType => hitVFXType;
    private int health;
    private bool isActive;
    private EnemyPool pool;
    public bool IsActive => isActive;
    public int PoolIndex { get; private set; }
    public void SetPoolIndex(int index)
    {
        PoolIndex = index;
    }
    
        // private void Start()
        // {
        //     surveyState = new SurveyState(this);
        //     moveState = new MoveToPointState(this);
        //     shootState = new ShootState(this);
        //     lungeState = new LungeState(this);
        //     SetState(surveyState);
        // }
        
        private void InitializeStates()
        {
            if (initialized)
                return;
            surveyState = new SurveyState(this);
            moveState = new MoveToPointState(this);
            shootState = new ShootState(this);
            lungeState = new LungeState(this);
            initialized = true;
        }
        
        private void AssignRefs()
        {
            if (player == null)
            {
                GameObject Player = GameObject.Find("Player");
                if (Player != null) player = Player.transform;
            }

            if (enemyBulletManager == null)
                enemyBulletManager = FindFirstObjectByType<EnemyBulletManager>();

            if (moveArea == null)
            {
                GameObject areaObj = GameObject.Find("RangerPointRange");
                if (areaObj != null)
                    moveArea = areaObj.GetComponent<PolygonCollider2D>();
            }
        }
    
        public void Activate(Vector2 position, Transform playerTarget, EnemyPool ownerPool)
        {
            InitializeStates();
            AssignRefs();
            transform.position = position;
            player = playerTarget;
            pool = ownerPool;
            health = maxHealth;
            isActive = true;
            
            cyclesCompleted = 0;
            shotsFired = 0;
            shotTimer = 0f;
            pendingState = null;

            // restart the state machine every spawn
            SetState(surveyState);
            gameObject.SetActive(true);
        }
    
    public void SetState(State state)
    {
        if (currentState != null)
            currentState.Exit();
        currentState = state;
        if (currentState != null)
            currentState.Enter();
    }
    
    public void RequestState(State state)
    {
        pendingState = state;
    }
    
    public void Tick(float deltaTime)
    {
        if (!isActive)
            return;
        dt = deltaTime;
        if (currentState != null)
            currentState.Execute();
        if (pendingState != null)
        {
            SetState(pendingState);
            pendingState = null;
        }
    }

    public void MoveTowardsTarget(float speed)
    {
        Vector2 pos = transform.position;
        Vector2 to = targetPoint - pos;

        if (to.sqrMagnitude < 0.0001f)
            return;

        Vector2 step = to.normalized * speed * dt;
        transform.position = pos + step;
    }
    
    public void BeginSurvey()
    {
        surveyTimer = surveySeconds;
    }

    public bool SurveyFinished()
    {
        surveyTimer -= dt;
        return surveyTimer <= 0f;
    }
    public bool TryPickNextShootPoint()
    {
        List<Vector2> points = ShootPointPicker.PickPointsInPolygon(
            moveArea,
            1,
            minDistanceFromLastPoint,
            maxPickAttempts
        );

        if (points.Count == 0)
            return false;

        targetPoint = points[0];
        return true;
    }

    // public void PickShootPoints()
    // {
    //     shootPoints = ShootPointPicker.PickPointsInPolygon(moveArea, shootPointCycles, minDistanceFromLastPoint, maxPickAttempts);
    // }

    public bool HasArrived()
    {
        Vector2 pos = transform.position;
        return (targetPoint - pos).sqrMagnitude <= arriveDistance * arriveDistance;
    }
    
    public void FireContinuously()
    {
        shotTimer -= dt;
        if (shotTimer > 0f)
            return;

        FireOneAtPlayer();
        shotsFired += 1;
        shotTimer = timeBetweenShots;
    }

    public bool ShootingCompleted()
    {
        return shotsFired >= bulletsPerBurst;
    }

    public void FinishShootWaveAndRequestNext()
    {
        cyclesCompleted += 1;

        if (cyclesCompleted >= shootPointCycles)
            RequestState(lungeState);
        else
            RequestState(surveyState);
    }

    private Vector2 GetFirePosition()
    {
        if (firePoint != null)
            return firePoint.position;

        return transform.position;
    }

    private void FireOneAtPlayer()
    {
        if (player == null || enemyBulletManager == null || enemyBulletType == null)
            return;

        Vector2 from = GetFirePosition();
        Vector2 dir = (Vector2)player.position - from;

        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector2.up;
        else
            dir.Normalize();
        enemyBulletManager.Spawn(from, dir);
    }

    public void LungeTowardsPlayer()
    {
        if (player == null)
            return;

        targetPoint = player.position;
        MoveTowardsTarget(lungeSpeed);
    }


    public void ResetBurst()
    {
        shotsFired = 0;
        shotTimer = 0f;
    }

    public void TakeDamage(int amount)
    {
        if (!isActive)
            return;

        health -= amount;
        if (health <= 0)
            Deactivate();
    }

    private void Deactivate()
    {
        if (!isActive)
            return;

        isActive = false;
        gameObject.SetActive(false);

        if (pool != null)
            pool.Return(this);
    }

}
