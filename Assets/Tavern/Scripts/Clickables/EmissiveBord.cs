using UnityEngine;

public class EmissiveBord : MonoBehaviour
{
    public sceneManager sceneManager;
    private Renderer[] childRenderers;
    private Material[] childMaterials;
    public float emissionIntensity = 0.5f; 
    Vector3 originalPos;
    public GameObject decoyTable;

    void Start()
    {
        originalPos = decoyTable.transform.position;
        childRenderers = GetComponentsInChildren<Renderer>();

        childMaterials = new Material[childRenderers.Length];

        for (int i = 0; i < childRenderers.Length; i++)
        {
            childMaterials[i] = childRenderers[i].material;
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            sceneManager.BackToRoom();
            decoyTable.transform.position = originalPos;
        }
    }
    void SetEmission(Color color, float intensity)
    {
        Color finalColor = color * intensity;

        for (int i = 0; i < childMaterials.Length; i++)
        {
            if (childMaterials[i] != null)
            {
                childMaterials[i].EnableKeyword("_EMISSION");
                childMaterials[i].SetColor("_EmissionColor", finalColor);

                DynamicGI.SetEmissive(childRenderers[i], finalColor);
            }
        }
    }
    void OnMouseEnter()
    {
        SetEmission(Color.gray, emissionIntensity);
    }
    void OnMouseExit()
    {
        SetEmission(Color.black, 0);
    }
    void OnMouseDown()
    {
        sceneManager.OpenUpgradeView();
        decoyTable.transform.position = new Vector3(0, -10, 0);
    }
}
