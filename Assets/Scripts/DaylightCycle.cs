using System;
using System.Buffers.Text;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
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

    [Header("Water Aesthetics (Layer Filter Mode)")]
    [Tooltip("Drop your Water Material asset directly from the Project/Assets folder here")]
    [SerializeField] private Material waterMaterialAsset;

    [Tooltip("The actual Water GameObject(s) in your hierarchy that need to change color visually")]
    [SerializeField] private Renderer[] waterSceneRenderers;

    [Tooltip("The single master color filter laid on top of all water properties")]
    [SerializeField] private Gradient masterWaterFilterGradient;

    [Range(0f, 1f)]
    [Tooltip("How strongly the filter layer is applied on top of the original colors")]
    [SerializeField] private float filterStrength = 0.6f;

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

    // Track the target and current intensity for the 1-second fade
    private float targetSunIntensity = 2f;
    private float currentSunIntensity = 2f;

    // Shader Property IDs
    private int shallowColorID;
    private int deepColorID;
    private int surfFoamColorID;
    private int interSecColorID;
    private int slColorID;
    private int underwaterColorID;
    private int shadowColorID;
    private int waveTopID;

    // Cached baseline default colors
    private Color origShallow, origDeep, origSurfFoam, origInterSec, origSl, origUnderwater, origShadow, origWaveTop;

    public float TimeValue { get; private set; }

    void Start()
    {
        sunLight = GetComponent<Light>();

        // Cache Shader IDs
        shallowColorID = Shader.PropertyToID("_Color_Shallow");
        deepColorID = Shader.PropertyToID("_Color_Deep");
        surfFoamColorID = Shader.PropertyToID("_SurfFoam_Color");
        interSecColorID = Shader.PropertyToID("_InterSec_Color");
        slColorID = Shader.PropertyToID("_SL_Color");
        underwaterColorID = Shader.PropertyToID("_Underwater_Color");
        shadowColorID = Shader.PropertyToID("_ShadowColor");
        waveTopID = Shader.PropertyToID("_Wave_Top_Color");

        if (waterMaterialAsset != null)
        {
            origShallow = waterMaterialAsset.GetColor(shallowColorID);
            origDeep = waterMaterialAsset.GetColor(deepColorID);
            origSurfFoam = waterMaterialAsset.GetColor(surfFoamColorID);
            origInterSec = waterMaterialAsset.GetColor(interSecColorID);
            origSl = waterMaterialAsset.GetColor(slColorID);
            origUnderwater = waterMaterialAsset.GetColor(underwaterColorID);
            origShadow = waterMaterialAsset.GetColor(shadowColorID);
            origWaveTop = waterMaterialAsset.GetColor(waveTopID);

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

        // Force snap values instantly at start frame
        UpdateCycleValues();
        currentSunIntensity = targetSunIntensity;
        if (sunLight != null) sunLight.intensity = currentSunIntensity;

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

        // UI Updates
        DetermineTargetUIState();
        AnimateImageColors();
        RotateUiElement(currentTimeInSeconds / totalCycleInSeconds);
    }

    private void UpdateCycleValues()
    {
        float overallDayPercent = currentTimeInSeconds / totalCycleInSeconds;

        currentInGameTime = overallDayPercent * 24f;
        UpdateDigitalClockString();

        // Standard sun rotation math
        float xRotation = (overallDayPercent * 360f) - 90f;
        transform.rotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 1-SECOND SNAP FADE TIMING:
        // Check if the current clock time is within the daytime window (6:00 to 18:00)
        if (currentInGameTime >= 6f && currentInGameTime < 18f)
        {
            targetSunIntensity = 2f; // Day target
        }
        else
        {
            targetSunIntensity = 0f; // Night target
        }

        if (sunLight != null)
        {
            sunLight.color = sunColorGradient.Evaluate(overallDayPercent);

            // To transition a total value of 2 units in exactly 1 second, 
            // the transition speed needs to be 2f units per second.
            float fadeSpeed = 2f;
            currentSunIntensity = Mathf.MoveTowards(currentSunIntensity, targetSunIntensity, fadeSpeed * Time.deltaTime);

            sunLight.intensity = currentSunIntensity;
        }

        // Keep water shader matching the back-and-forth movement layout
        TimeValue = Mathf.Cos((overallDayPercent - 0.5f) * 2f * Mathf.PI);
        float waterSwayProgress = (TimeValue * 0.5f) + 0.5f;
        UpdateWaterLayerFilter(waterSwayProgress);
    }

    private void UpdateWaterLayerFilter(float progress)
    {
        if (runtimeWaterMaterial == null) return;

        Color filterColor = masterWaterFilterGradient.Evaluate(progress);

        runtimeWaterMaterial.SetColor(shallowColorID, Color.Lerp(origShallow, filterColor, filterStrength));
        runtimeWaterMaterial.SetColor(deepColorID, Color.Lerp(origDeep, filterColor, filterStrength));
        runtimeWaterMaterial.SetColor(surfFoamColorID, Color.Lerp(origSurfFoam, filterColor, filterStrength));
        runtimeWaterMaterial.SetColor(interSecColorID, Color.Lerp(origInterSec, filterColor, filterStrength));
        runtimeWaterMaterial.SetColor(slColorID, Color.Lerp(origSl, filterColor, filterStrength));
        runtimeWaterMaterial.SetColor(underwaterColorID, Color.Lerp(origUnderwater, filterColor, filterStrength));
        runtimeWaterMaterial.SetColor(shadowColorID, Color.Lerp(origShadow, filterColor, filterStrength));
        runtimeWaterMaterial.SetColor(waveTopID, Color.Lerp(origWaveTop, filterColor, filterStrength));
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