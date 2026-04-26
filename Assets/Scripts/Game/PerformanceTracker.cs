using System.Collections.Generic;
using UnityEngine;

// tracks hits received in the last 30 seconds
// performanceScore: 0=struggling, 1=doing great which is fed into StarfieldController
public class PerformanceTracker : MonoBehaviour, ITickable
{
    [SerializeField] private PlayerLifeSystem  lifeSystem;
    [SerializeField] private StarfieldController starfield;

    [SerializeField] private float windowSeconds   = 30f;
    [SerializeField] private int   hitsForWorstScore = 6; 

    // timestamps of recent hits 
    private readonly Queue<float> hitTimestamps = new Queue<float>();
    // accumulated time for rolling window, uses dt
    private float elapsed;


    public float PerformanceScore { get; private set; } = 1f;

    private void OnEnable()
    {
        if (lifeSystem != null)
            lifeSystem.OnHitReceived += OnHit;
    }

    private void OnDisable()
    {
        if (lifeSystem != null)
            lifeSystem.OnHitReceived -= OnHit;
    }
    
    private void Start()
    {
        if (GameManager.instance != null)
            GameManager.instance.RegisterTickable(this);
    }


    private void OnHit()
    {
        hitTimestamps.Enqueue(Time.time);
    }

    public void Tick(float dt)
    {
        elapsed += dt;
        PruneOldHits();
        PerformanceScore = CalculateScore();
        if (starfield != null) starfield.SetPerformanceScore(PerformanceScore);
    }

    private void PruneOldHits()
    {
        float cutoff = Time.time - windowSeconds;
        while (hitTimestamps.Count > 0 && hitTimestamps.Peek() < cutoff)
            hitTimestamps.Dequeue();
    }

    private float CalculateScore()
    {
        float ratio = (float)hitTimestamps.Count / hitsForWorstScore;
        return Mathf.Clamp01(1f - ratio);
    }
}
