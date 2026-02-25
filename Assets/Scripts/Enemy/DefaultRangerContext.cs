using System.Collections.Generic;
using UnityEngine;

public class DefaultRangerContext : MonoBehaviour, ITickable
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
    
    [SerializeField] private Transform player;
    [SerializeField] private BulletManager bulletManager;
    [SerializeField] private BulletTypeSO enemyBulletType;
    [SerializeField] private Transform firePoint;
    [SerializeField] private PolygonCollider2D moveArea;
    
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

    [Header("Shoot Pattern")]
    [SerializeField] private int shootPointCycles = 3;
    [SerializeField] private int bulletsPerBurst = 5;
    [SerializeField] private float timeBetweenShots = 0.12f;
    
    private float dt;
    private Vector2 targetPoint;

    private int cyclesCompleted;
    private int shotsFired;
    private float shotTimer;
    
        private void Start()
        {
            surveyState = new SurveyState(this);
            moveState = new MoveToPointState(this);
            shootState = new ShootState(this);
            lungeState = new LungeState(this);
            SetState(surveyState);
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
        dt = deltaTime;
        if (currentState != null)
            currentState.Execute();
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

    public void PickShootPoints()
    {
        shootPoints = ShootPointPicker.PickPointsInPolygon(moveArea, shootPointCycles, minDistanceFromLastPoint, maxPickAttempts);
    }

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
        if (player == null || bulletManager == null || enemyBulletType == null)
            return;

        Vector2 from = GetFirePosition();
        Vector2 dir = (Vector2)player.position - from;

        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector2.up;
        else
            dir.Normalize();
        bulletManager.SpawnBullet(enemyBulletType, from, dir);
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


}
