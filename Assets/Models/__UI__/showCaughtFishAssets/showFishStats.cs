using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class showFishStats : MonoBehaviour
{
    public TextMeshProUGUI fishNameText;
    public TextMeshProUGUI fishWeightText;
    public TextMeshProUGUI fishLengthText;

    public GameObject panel;
    private bool active = false;
    public Color[] rarityColors;

    void Start()
    {
        setPanelVis(false);
    }
    public void displayFishInfo(string fishname, float fishWeight, float fishLenght, int fishRarity)
    {
        fishNameText.text = $"{fishname}";
        ApplyRareFishGradient(colorFromRarity(fishRarity), colorFromRarity(math.min(10, fishRarity + 1)));
        fishWeightText.text = $"{fishWeight:F1} Kg";
        fishLengthText.text = $"{fishLenght:F1} Cm";
        setPanelVis(true);
    }
    public void hideFishInfo()
    {
        setPanelVis(false);
    }
    private void setPanelVis(bool state)
    {
        active = state;
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }
    private void ApplyRareFishGradient(Color colorOne, Color colorTwo)
    {
        fishNameText.enableVertexGradient = true;
        fishNameText.colorGradient = new VertexGradient(colorOne, colorOne, colorTwo, colorTwo);
    }
    private Color colorFromRarity(int fishRarity)
    {
        //return rarityColors[fishRarity -1];
        return new Color(UnityEngine.Random.Range(0F,1F), UnityEngine.Random.Range(0, 1F), UnityEngine.Random.Range(0, 1F));

    }
}
