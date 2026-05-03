using UnityEngine;

public class AudioWaveVisualizer : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Wave")]
    [SerializeField] private float waveWidth = 4f;
    [SerializeField] private float waveHeight = 1f;
    [Range(32, 512)]
    [SerializeField] private int sampleCount = 128;

    [Header("Line")]
    [SerializeField] private float lineStartWidth = 0.04f;
    [SerializeField] private float lineEndWidth = 0.04f;
    [SerializeField] private Color lineColor = Color.cyan;

    [Header("Animation")]
    [Range(0.01f, 1f)]
    [SerializeField] private float smoothing = 0.3f;
    [SerializeField] private float amplitudeMultiplier = 2f;
    [SerializeField] private float minIdleAmplitude = 0.02f;
    [SerializeField] private float idleFrequency = 1.5f;

    [Header("Position")]
    [SerializeField] private Vector3 centerPosition = Vector3.zero;
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
        SoundManager sm = FindFirstObjectByType<SoundManager>();
        if (sm != null)
        {
            if (audioSource != null)
            audioSource = sm.GetComponent<AudioSource>();
        }
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
            audioSource.GetOutputData(samples, 0);
            for (int i = 0; i < sampleCount; i++)
            {
                float raw = samples[i] * amplitudeMultiplier;
                smoothedSamples[i] = Mathf.Lerp(smoothedSamples[i], raw, smoothing);
            }
        }
        else
        {
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
            float t = (float)i / (sampleCount - 1); 
            float x = centerPosition.x + (t - 0.5f) * waveWidth;
            float y = centerPosition.y + smoothedSamples[i] * waveHeight;
            float z = centerPosition.z;
            positions[i] = new Vector3(x, y, z);
        }
    }

    private void UpdateLineAppearance()
    {
        lineRenderer.startWidth = lineStartWidth;
        lineRenderer.endWidth   = lineEndWidth;
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor   = lineColor;
        if (lineRenderer.material != null)
            lineRenderer.material.color = lineColor;
    }
    
    public void SetAudioSource(AudioSource source)
    {
        audioSource = source;
    }
    public void SetVisible(bool visible)
    {
        lineRenderer.enabled = visible;
    }
    
    public void SetPosition(Vector3 position)
    {
        centerPosition = position;
    }
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
    private void OnValidate()
    {
        if (lineRenderer == null) return;
        if (samples == null || samples.Length != sampleCount) InitArrays();
        lineRenderer.positionCount = sampleCount;
        SetupLineRenderer();
    }
}
