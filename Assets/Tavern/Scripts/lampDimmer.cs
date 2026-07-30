using UnityEngine;

[RequireComponent(typeof(Light))]
public class OilLampFlicker : MonoBehaviour
{
    [Header("Intensity Settings")]
    [Tooltip("Base brightness of the lamp.")]
    public float baseIntensity = 20f;
    [Tooltip("Maximum deviation from base intensity.")]
    public float intensityVariance = 5f;
    [Tooltip("How fast the flame flickers.")]
    public float flickerSpeed = 0.5f;

    [Header("Range Settings")]
    [Tooltip("Base light range/reach.")]
    public float baseRange = 15f;
    [Tooltip("Maximum deviation from base range.")]
    public float rangeVariance = 5f;

    [Header("Color Dynamics")]
    [Tooltip("Deeper red for lower flame output / embers.")]
    public Color coolColor = new Color(0.9f, 0.25f, 0.05f);
    [Tooltip("Main burning amber color.")]
    public Color midColor = new Color(1.0f, 0.55f, 0.1f);
    [Tooltip("Bright yellow flare color.")]
    public Color hotColor = new Color(1.0f, 0.85f, 0.3f);
    [Tooltip("How fast the color palette shifts.")]
    public float colorShiftSpeed = 0.25f;

    [Header("Flame Jitter (Flame Movement)")]
    [Tooltip("Enable tiny position shifts to simulate the physical flame moving on the wick.")]
    public bool enableJitter = true;
    public float jitterAmount = 0.01f;

    private Light lampLight;
    private Vector3 initialPosition;
    private float seed;

    void Start()
    {
        lampLight = GetComponent<Light>();
        lampLight.type = LightType.Point;
        initialPosition = transform.localPosition;

        seed = Random.Range(0f, 100f);
    }

    void Update()
    {
        float time = Time.time * flickerSpeed;

        float noiseIntensity = Mathf.PerlinNoise(seed, time);
        lampLight.intensity = baseIntensity + (noiseIntensity - 0.5f) * 2f * intensityVariance;

        float noiseRange = Mathf.PerlinNoise(seed + 10f, time * 0.8f);
        lampLight.range = baseRange + (noiseRange - 0.5f) * 2f * rangeVariance;

        float colorNoise = Mathf.PerlinNoise(seed + 50f, Time.time * colorShiftSpeed);
        if (colorNoise < 0.5f)
        {
            lampLight.color = Color.Lerp(coolColor, midColor, colorNoise * 2f);
        }
        else
        {
            lampLight.color = Color.Lerp(midColor, hotColor, (colorNoise - 0.5f) * 2f);
        }

        if (enableJitter)
        {
            Vector3 offset = new Vector3(
                (Mathf.PerlinNoise(seed + 20f, time) - 0.5f) * jitterAmount,
                (Mathf.PerlinNoise(seed + 30f, time) - 0.5f) * jitterAmount,
                (Mathf.PerlinNoise(seed + 40f, time) - 0.5f) * jitterAmount
            );
            transform.localPosition = initialPosition + offset;
        }
    }
}