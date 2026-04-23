using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class StarfieldController : MonoBehaviour
{
    [Header("Colors")]
    [SerializeField] private Color[] starColors = new Color[6];

    [Header("Flicker")]
    [SerializeField] private float minFlickerSpeed = 0.3f;
    [SerializeField] private float maxFlickerSpeed = 1.2f;
    [SerializeField] private float minBrightness = 0.3f;
    [SerializeField] private float maxBrightness = 1f;

    private float performanceScore = 1f;
    private bool proceduralEnabled = true;
    private Color[] tintedColors;

    private ParticleSystem stars;
    private ParticleSystem.Particle[] particles;
    private float[] timeOffsets;
    private float[] flickerSpeeds;
    private float[] colorSpeeds;
    private int[] colorIndices;
    public void ApplySettings(SettingsData data)
    {
        proceduralEnabled = data.proceduralBgEnabled;
    }
    public void SetPerformanceScore(float score)
    {
        performanceScore = Mathf.Clamp01(score);
        RebuildTintedPalette();
    }

    private void Start()
    {
        stars = GetComponent<ParticleSystem>();
        if (MirageSaveSystem.Instance != null)
            ApplySettings(MirageSaveSystem.Instance.LoadSettingsOrDefault());

        tintedColors = (Color[])starColors.Clone();
        StartCoroutine(InitParticles());
    }

    System.Collections.IEnumerator InitParticles()
    {
        yield return null;
        int count = stars.particleCount;
        particles     = new ParticleSystem.Particle[count];
        timeOffsets   = new float[count];
        flickerSpeeds = new float[count];
        colorSpeeds   = new float[count];
        colorIndices  = new int[count];
        for (int i = 0; i < count; i++)
        {
            timeOffsets[i]   = Random.Range(0f, 100f);
            flickerSpeeds[i] = Random.Range(minFlickerSpeed, maxFlickerSpeed);
            colorSpeeds[i]   = Random.Range(0.05f, 0.2f);
            colorIndices[i]  = Random.Range(0, starColors.Length);
        }
        RebuildTintedPalette();
    }

    void LateUpdate()
    {
        if (particles == null) return;
        int count = stars.GetParticles(particles);
        for (int i = 0; i < count; i++)
        {
            if (i >= timeOffsets.Length) break;
            float t = Time.time * flickerSpeeds[i] + timeOffsets[i];
            float brightness = Mathf.Lerp(minBrightness, maxBrightness,
                (Mathf.Sin(t * Mathf.PI * 2f) + 1f) * 0.5f);
            float colorT = (Mathf.Sin(Time.time * colorSpeeds[i] + timeOffsets[i]) + 1f) * 0.5f;
            int indexA = colorIndices[i];
            int indexB = (indexA + 1) % tintedColors.Length;
            Color baseColor = Color.Lerp(tintedColors[indexA], tintedColors[indexB], colorT);
            particles[i].startColor = baseColor * brightness;
        }
        stars.SetParticles(particles, count);
    }
    private void RebuildTintedPalette()
    {
        if (starColors == null || tintedColors == null) return;

        if (!proceduralEnabled)
        {
            System.Array.Copy(starColors, tintedColors, starColors.Length);
            return;
        }

        for (int i = 0; i < starColors.Length; i++)
        {
            Color vivid = starColors[i];
            Color.RGBToHSV(vivid, out float h, out float s, out float v);
            float newS = Mathf.Lerp(0f, s, performanceScore);
            float newV = Mathf.Lerp(0.15f, v, performanceScore);
            tintedColors[i] = Color.HSVToRGB(h, newS, newV);
        }
    }
}