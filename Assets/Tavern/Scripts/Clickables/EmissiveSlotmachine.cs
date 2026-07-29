using UnityEngine;

public class EmissiveSlotmachine : MonoBehaviour
{
    public sceneManager sceneManager;
    private Renderer targetRenderer;
    private Material targetMaterial;
    Vector3 originalPos;
    public GameObject decoySlotMachine;
    public GameObject[] slotmachineWheels;
    public GameObject[] decoySlotmachineWheels;
    public float emissionIntensity = 0.1f;
    void Start()
    {
        targetRenderer = GetComponent<Renderer>();
        
        targetMaterial = targetRenderer.material;
        originalPos = decoySlotMachine.transform.position;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            sceneManager.BackToRoom();
            decoySlotMachine.transform.position = originalPos;
        }
        for (int i = 0; i < 2; i++)
        {
            decoySlotmachineWheels[i].transform.rotation = slotmachineWheels[i].transform.rotation;
        }
    }
    void SetEmission(Color color, float intensity)
    {
        targetMaterial.EnableKeyword("_EMISSION");

        Color finalColor = color * intensity;

        targetMaterial.SetColor("_EmissionColor", finalColor);

        DynamicGI.SetEmissive(targetRenderer, finalColor);
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
        sceneManager.OpenSlotMachineView();
        decoySlotMachine.transform.position = new Vector3(0,-10,0);
    }
    void OnEnable()
    {
        
    }
}
