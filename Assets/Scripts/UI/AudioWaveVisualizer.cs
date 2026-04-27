using UnityEngine;

// Standalone audio waveform visualizer using LineRenderer.
// Attach to any GameObject in your Game scene.
// Reads from SoundManager's music AudioSource and draws a live waveform.
//
// Setup:
//   1. Add this script to an empty GameObject
//   2. Add a LineRenderer component to the same GameObject
//   3. Assign the LineRenderer in the inspector
//   4. Assign the AudioSource playing your background music
//   5. Tune width, height, and sample count in inspector
[RequireComponent(typeof(LineRenderer))]
public class AudioWaveVisualizer : MonoBehaviour
{
    [Header("Audio")]
    [Tooltip("The AudioSource playing background music. Leave empty to auto-find from SoundManager.")]
    [SerializeField] private AudioSource audioSource;

    [Header("Waveform Shape")]
    [Tooltip("Total width of the waveform in world units")]
    [SerializeField] private float waveWidth = 4f;

    [Tooltip("Maximum height of the waveform peaks in world units")]
    [SerializeField] private float waveHeight = 1f;

    [Tooltip("How many sample points to draw — higher = smoother but more expensive")]
    [Range(32, 512)]
    [SerializeField] private int sampleCount = 128;

    [Header("Line Appearance")]
    [Tooltip("Thickness of the line at its start")]
    [SerializeField] private float lineStartWidth = 0.04f;

    [Tooltip("Thickness of the line at its end")]
    [SerializeField] private float lineEndWidth = 0.04f;

    [Tooltip("Colour of the waveform line")]
    [SerializeField] private Color lineColor = Color.cyan;

    [Header("Animation")]
    [Tooltip("How quickly the waveform smooths between samples — lower = more responsive, higher = smoother")]
    [Range(0.01f, 1f)]
    [SerializeField] private float smoothing = 0.3f;

    [Tooltip("Multiplier applied to the raw sample amplitude — boost this if the waveform looks flat")]
    [SerializeField] private float amplitudeMultiplier = 2f;

    [Tooltip("Minimum wave height so the line is always visible even in silence")]
    [SerializeField] private float minIdleAmplitude = 0.02f;

    [Tooltip("Idle sine wave frequency when audio is silent")]
    [SerializeField] private float idleFrequency = 1.5f;

    [Header("Position")]
    [Tooltip("World position of the centre of the waveform")]
    [SerializeField] private Vector3 centerPosition = Vector3.zero;

    [Tooltip("Sorting order for the LineRenderer — raise to draw above game sprites")]
    [SerializeField] private int sortingOrder = 10;

    private LineRenderer lineRenderer;
    private float[]      samples;
    private float[]      smoothedSamples;
    private Vector3[]    positions;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        SetupLineRenderer();
        InitArrays();

        // auto-find AudioSource from SoundManager if not assigned
        if (audioSource == null)
            TryFindAudioSource();
    }

    private void SetupLineRenderer()
    {
        lineRenderer.positionCount  = sampleCount;
        lineRenderer.startWidth     = lineStartWidth;
        lineRenderer.endWidth       = lineEndWidth;
        lineRenderer.startColor     = lineColor;
        lineRenderer.endColor       = lineColor;
        lineRenderer.useWorldSpace  = true;
        lineRenderer.sortingOrder   = sortingOrder;

        // use a simple unlit material so it glows cleanly
        if (lineRenderer.material == null || lineRenderer.material.name == "Default-Line")
        {
            Material mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = lineColor;
            lineRenderer.material = mat;
        }
        else
        {
            lineRenderer.material.color = lineColor;
        }
    }

    private void InitArrays()
    {
        samples         = new float[sampleCount];
        smoothedSamples = new float[sampleCount];
        positions       = new Vector3[sampleCount];
    }

    private void TryFindAudioSource()
    {
        // try to grab the music source from SoundManager
        SoundManager sm = FindFirstObjectByType<SoundManager>();
        if (sm != null)
        {
            // SoundManager stores music in its AudioSource component
            audioSource = sm.GetComponent<AudioSource>();
            if (audioSource != null)
                Debug.Log("AudioWaveVisualizer: found AudioSource on SoundManager");
        }

        if (audioSource == null)
            Debug.LogWarning("AudioWaveVisualizer: no AudioSource found — assign one in inspector");
    }

    private void Update()
    {
        UpdateSamples();
        BuildPositions();
        lineRenderer.SetPositions(positions);
        UpdateLineAppearance();
    }

    private void UpdateSamples()
    {
        if (audioSource != null && audioSource.isPlaying && audioSource.clip != null)
        {
            // get raw waveform data at current playback position
            audioSource.GetOutputData(samples, 0);

            // smooth each sample toward the raw value
            for (int i = 0; i < sampleCount; i++)
            {
                float raw = samples[i] * amplitudeMultiplier;
                smoothedSamples[i] = Mathf.Lerp(smoothedSamples[i], raw, smoothing);
            }
        }
        else
        {
            // idle animation — gentle sine wave when no audio playing
            float t = Time.time * idleFrequency;
            for (int i = 0; i < sampleCount; i++)
            {
                float phase = (float)i / sampleCount * Mathf.PI * 2f;
                float idle  = Mathf.Sin(t + phase) * minIdleAmplitude;
                smoothedSamples[i] = Mathf.Lerp(smoothedSamples[i], idle, smoothing);
            }
        }
    }

    private void BuildPositions()
    {
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / (sampleCount - 1); // 0..1 across width
            float x = centerPosition.x + (t - 0.5f) * waveWidth;
            float y = centerPosition.y + smoothedSamples[i] * waveHeight;
            float z = centerPosition.z;
            positions[i] = new Vector3(x, y, z);
        }
    }

    private void UpdateLineAppearance()
    {
        // keep line appearance in sync with inspector changes at runtime
        lineRenderer.startWidth = lineStartWidth;
        lineRenderer.endWidth   = lineEndWidth;
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor   = lineColor;
        if (lineRenderer.material != null)
            lineRenderer.material.color = lineColor;
    }

    // ── Public API ────────────────────────────────────────────────────────

    // Call this to repoint the visualizer at a different AudioSource at runtime
    public void SetAudioSource(AudioSource source)
    {
        audioSource = source;
    }

    // Show or hide the waveform
    public void SetVisible(bool visible)
    {
        lineRenderer.enabled = visible;
    }

    // Animate the center position — useful for moving the wave around the screen
    public void SetPosition(Vector3 position)
    {
        centerPosition = position;
    }

    // Pulse the height — call this from gameplay events for reactive visuals
    public void PulseAmplitude(float multiplier, float duration)
    {
        StartCoroutine(PulseRoutine(multiplier, duration));
    }

    private System.Collections.IEnumerator PulseRoutine(float multiplier, float duration)
    {
        float original = amplitudeMultiplier;
        amplitudeMultiplier *= multiplier;
        yield return new WaitForSeconds(duration);
        amplitudeMultiplier = original;
    }

    // ── Editor validation ─────────────────────────────────────────────────

    private void OnValidate()
    {
        if (lineRenderer == null) return;
        if (samples == null || samples.Length != sampleCount) InitArrays();
        lineRenderer.positionCount = sampleCount;
        SetupLineRenderer();
    }
}
