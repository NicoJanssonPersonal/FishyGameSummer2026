using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DayNightCycle dayNightCycle;
    [SerializeField] private Light lamp;

    [Header("Settings")]
    public float lightsFadeOnTime = 17f;
    public float lightsFadeOffTime = 6f;
    public float maxLightIntensity = 100f;

    private void Awake()
    {
        if (lamp == null)
        {
            lamp = GetComponentInChildren<Light>();
        }

        if (dayNightCycle == null)
        {
            dayNightCycle = FindAnyObjectByType<DayNightCycle>();
        }
    }

    private void Update()
    {
        TurnOnOrOffLights(lamp);
    }

    public void TurnOnOrOffLights(Light light)
    {
        if (light == null || dayNightCycle == null) return;

        float time = dayNightCycle.currentInGameTime;
        float calculatedIntensity = 0f;
        float calculatedShadowStrength = 0f;

        if (time >= lightsFadeOnTime && time < lightsFadeOnTime + 1)
        {
            float t = time - lightsFadeOnTime;
            calculatedIntensity = Mathf.Lerp(0f, maxLightIntensity, t);
            calculatedShadowStrength = Mathf.Lerp(0f, 1f, t);
        }
        else if (time >= lightsFadeOnTime + 1 || time < lightsFadeOffTime)
        {
            calculatedIntensity = maxLightIntensity;
            calculatedShadowStrength = 1f;
        }
        else if (time >= lightsFadeOffTime && time < lightsFadeOffTime + 1)
        {
            float t = time - lightsFadeOffTime;
            calculatedIntensity = Mathf.Lerp(maxLightIntensity, 0f, t);
            calculatedShadowStrength = Mathf.Lerp(1f, 0f, t);
        }
        else
        {
            calculatedIntensity = 0f;
            calculatedShadowStrength = 0f;
        }

        light.intensity = calculatedIntensity;
        light.shadowStrength = calculatedShadowStrength;
    }
}