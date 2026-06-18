using UnityEngine;
using UnityEngine.UI;

public class DayNightCycle : MonoBehaviour
{
    [Header("Cycle Duration (in minutes)")]
    [SerializeField] private float targetDayLengthInMinutes = 6f;
    [SerializeField] private float targetNightLengthInMinutes = 6f;

    [Header("Starting Time")]
    [Range(0f, 23.99f)]
    [Tooltip("The in-game hour the scene should start at (0 = Midnight, 12 = Noon)")]
    [SerializeField] private float startHour = 12f;

    [Header("Sun Aesthetics")]
    [SerializeField] private Gradient sunColorGradient;

    [Header("Water Aesthetics (Individual Gradients Mode)")]
    [Tooltip("Drop your Water Material asset directly from the Project/Assets folder here")]
    [SerializeField] private Material waterMaterialAsset;

    [Tooltip("The actual Water GameObject(s) in your hierarchy that need to change color visually")]
    [SerializeField] private Renderer[] waterSceneRenderers;

    [Header("Prefab Material Sync (Direct Asset Mode)")]
    [Tooltip("Drag the Material ASSET directly from your Project window (Assets folder) here")]
    [SerializeField] private Material prefabMaterialAsset;
    [Tooltip("The exact case-sensitive reference name of the color property in your shader (e.g., _Color, _BaseColor, or _plop_color)")]
    [SerializeField] private string extraMaterialColorPropertyName = "_Color";
    [Tooltip("Dedicated color gradient for your prefab material")]
    [SerializeField] private Gradient plopColorGradient;

    [SerializeField] private Gradient shallowColorGradient;
    [SerializeField] private Gradient deepColorGradient;
    [SerializeField] private Gradient surfFoamColorGradient;
    [SerializeField] private Gradient interSecColorGradient;
    [SerializeField] private Gradient slColorGradient;
    [SerializeField] private Gradient underwaterColorGradient;
    [SerializeField] private Gradient shadowColorGradient;
    [SerializeField] private Gradient waveTopGradient;

    [Header("UI Image Fading")]
    [SerializeField] private Image dayImage;
    [SerializeField] private Image nightImage;
    [Tooltip("How fast the UI fades in real-time seconds")]
    [SerializeField] private float fadeDurationInSeconds = 1f;

    [Header("UI Image Rotation")]
    [Tooltip("The UI Image that will rotate with the time")]
    [SerializeField] private RectTransform rotatingUiImage;
    [Tooltip("Check this if the UI image rotates backwards")]
    [SerializeField] private bool invertRotation = false;

    [Header("Developer Time Readout")]
    [Tooltip("Current time on a 24-hour scale (0.0 to 24.0)")]
    public float currentInGameTime;

    [SerializeField] private string digitalClockDisplay;

    public Light sunLight;
    private Material runtimeWaterMaterial;
    private float totalCycleInSeconds;
    private float currentTimeInSeconds;

    private float targetNightAlpha = 0f;
    private float currentFadeAlpha = 0f;

    // Shader Property IDs
    private int shallowColorID;
    private int deepColorID;
    private int surfFoamColorID;
    private int interSecColorID;
    private int slColorID;
    private int underwaterColorID;
    private int shadowColorID;
    private int waveTopID;
    private int extraMaterialColorID;

    public float TimeValue { get; private set; }

    void Start()
    {
        if (sunLight == null) sunLight = GetComponent<Light>();

        // Cache Shader IDs
        shallowColorID = Shader.PropertyToID("_Color_Shallow");
        deepColorID = Shader.PropertyToID("_Color_Deep");
        surfFoamColorID = Shader.PropertyToID("_SurfFoam_Color");
        interSecColorID = Shader.PropertyToID("_InterSec_Color");
        slColorID = Shader.PropertyToID("_SL_Color");
        underwaterColorID = Shader.PropertyToID("_Underwater_Color");
        shadowColorID = Shader.PropertyToID("_ShadowColor");
        waveTopID = Shader.PropertyToID("_Wave_Top_Color");

        // Cache the custom material property ID
        extraMaterialColorID = Shader.PropertyToID(extraMaterialColorPropertyName);

        // Setup Runtime Water Material
        if (waterMaterialAsset != null)
        {
            runtimeWaterMaterial = new Material(waterMaterialAsset);

            if (waterSceneRenderers != null && waterSceneRenderers.Length > 0)
            {
                foreach (Renderer rend in waterSceneRenderers)
                {
                    if (rend != null) rend.material = runtimeWaterMaterial;
                }
            }
        }

        totalCycleInSeconds = (targetDayLengthInMinutes + targetNightLengthInMinutes) * 60f;
        currentTimeInSeconds = (startHour / 24f) * totalCycleInSeconds;

        UpdateCycleValues();

        DetermineTargetUIState();
        currentFadeAlpha = targetNightAlpha;
        SetImageAlpha(nightImage, currentFadeAlpha);
        SetImageAlpha(dayImage, 1f - currentFadeAlpha);
    }

    void Update()
    {
        totalCycleInSeconds = (targetDayLengthInMinutes + targetNightLengthInMinutes) * 60f;

        currentTimeInSeconds += Time.deltaTime;
        if (currentTimeInSeconds >= totalCycleInSeconds)
        {
            currentTimeInSeconds = 0f;
        }

        UpdateCycleValues();
        DetermineTargetUIState();
        AnimateImageColors();
        RotateUiElement(currentTimeInSeconds / totalCycleInSeconds);
    }

    private void UpdateCycleValues()
    {
        float overallDayPercent = currentTimeInSeconds / totalCycleInSeconds;

        currentInGameTime = overallDayPercent * 24f;
        UpdateDigitalClockString();

        float xRotation = (overallDayPercent * 360f) - 90f;
        transform.rotation = Quaternion.Euler(xRotation, 0f, 0f);

        TimeValue = Mathf.Cos((overallDayPercent - 0.5f) * 2f * Mathf.PI);
        float waterSwayProgress = (TimeValue * 0.5f) + 0.5f;

        if (sunLight != null)
        {
            sunLight.color = sunColorGradient.Evaluate(waterSwayProgress);

            float calculatedIntensity = 0f;
            float calculatedShadowStrength = 0f;

            if (currentInGameTime >= 6f && currentInGameTime < 7f)
            {
                float t = currentInGameTime - 6f;
                calculatedIntensity = Mathf.Lerp(0f, 2f, t);
                calculatedShadowStrength = Mathf.Lerp(0f, 1f, t);
            }
            else if (currentInGameTime >= 7f && currentInGameTime < 17f)
            {
                calculatedIntensity = 2f;
                calculatedShadowStrength = 1f;
            }
            else if (currentInGameTime >= 17f && currentInGameTime < 18f)
            {
                float t = currentInGameTime - 17f;
                calculatedIntensity = Mathf.Lerp(2f, 0f, t);
                calculatedShadowStrength = Mathf.Lerp(1f, 0f, t);
            }
            else
            {
                calculatedIntensity = 0f;
                calculatedShadowStrength = 0f;
            }

            sunLight.intensity = calculatedIntensity;
            sunLight.shadowStrength = calculatedShadowStrength;
        }

        UpdateWaterLayerFilter(waterSwayProgress);
    }

    private void UpdateWaterLayerFilter(float progress)
    {
        if (runtimeWaterMaterial != null)
        {
            if (shallowColorGradient != null) runtimeWaterMaterial.SetColor(shallowColorID, shallowColorGradient.Evaluate(progress));
            if (deepColorGradient != null) runtimeWaterMaterial.SetColor(deepColorID, deepColorGradient.Evaluate(progress));
            if (surfFoamColorGradient != null) runtimeWaterMaterial.SetColor(surfFoamColorID, surfFoamColorGradient.Evaluate(progress));
            if (interSecColorGradient != null) runtimeWaterMaterial.SetColor(interSecColorID, interSecColorGradient.Evaluate(progress));
            if (slColorGradient != null) runtimeWaterMaterial.SetColor(slColorID, slColorGradient.Evaluate(progress));
            if (underwaterColorGradient != null) runtimeWaterMaterial.SetColor(underwaterColorID, underwaterColorGradient.Evaluate(progress));
            if (shadowColorGradient != null) runtimeWaterMaterial.SetColor(shadowColorID, shadowColorGradient.Evaluate(progress));
            if (waveTopGradient != null) runtimeWaterMaterial.SetColor(waveTopID, waveTopGradient.Evaluate(progress));
        }

        // Directly forces the color change to the project material asset file 
        if (prefabMaterialAsset != null && plopColorGradient != null)
        {
            prefabMaterialAsset.SetColor(extraMaterialColorID, plopColorGradient.Evaluate(progress));
        }
    }

    private void DetermineTargetUIState()
    {
        if (currentInGameTime >= 21f || currentInGameTime < 7f)
        {
            targetNightAlpha = 1f;
        }
        else
        {
            targetNightAlpha = 0f;
        }
    }

    private void AnimateImageColors()
    {
        if (dayImage == null || nightImage == null) return;

        float fadeSpeed = 1f / fadeDurationInSeconds;
        currentFadeAlpha = Mathf.MoveTowards(currentFadeAlpha, targetNightAlpha, fadeSpeed * Time.deltaTime);

        SetImageAlpha(nightImage, currentFadeAlpha);
        SetImageAlpha(dayImage, 1f - currentFadeAlpha);
    }

    private void SetImageAlpha(Image targetImage, float alpha)
    {
        if (targetImage == null) return;

        Color tempColor = targetImage.color;
        tempColor.a = alpha;
        targetImage.color = tempColor;
    }

    private void RotateUiElement(float progress)
    {
        if (rotatingUiImage == null) return;

        float zRotation = progress * 360f;
        if (!invertRotation)
        {
            zRotation = -zRotation;
        }
        rotatingUiImage.localRotation = Quaternion.Euler(0f, 0f, zRotation);
    }

    private void UpdateDigitalClockString()
    {
        int hours = Mathf.FloorToInt(currentInGameTime);
        int minutes = Mathf.FloorToInt((currentInGameTime - hours) * 60f);
        digitalClockDisplay = string.Format("{0:00}:{1:00}", hours, minutes);
    }
}