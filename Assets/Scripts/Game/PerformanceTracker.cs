using System.Collections.Generic;
using UnityEngine;

// Tracks hits received in a 30-second rolling window and produces a
// PerformanceScore (0=struggling, 1=doing great) that feeds StarfieldController.
// PlayerLifeSystem calls NotifyHit() via its OnHitReceived Action.
public class PerformanceTracker : MonoBehaviour
{
    [SerializeField] private PlayerLifeSystem  lifeSystem;
    [SerializeField] private StarfieldController starfield;

    [SerializeField] private float windowSeconds   = 30f;
    [SerializeField] private int   hitsForWorstScore = 6; // 6+ hits in window = score 0

    // timestamps of recent hits — entries older than windowSeconds are pruned
    private readonly Queue<float> hitTimestamps = new Queue<float>();

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

    private void OnHit()
    {
        hitTimestamps.Enqueue(Time.time);
    }

    private void Update()
    {
        PruneOldHits();
        float score = CalculateScore();
        PerformanceScore = score;

        if (starfield != null)
            starfield.SetPerformanceScore(score);
    }

    private void PruneOldHits()
    {
        float cutoff = Time.time - windowSeconds;
        while (hitTimestamps.Count > 0 && hitTimestamps.Peek() < cutoff)
            hitTimestamps.Dequeue();
    }

    private float CalculateScore()
    {
        // fewer hits = higher score; clamped 0..1
        float ratio = (float)hitTimestamps.Count / hitsForWorstScore;
        return Mathf.Clamp01(1f - ratio);
    }
}
