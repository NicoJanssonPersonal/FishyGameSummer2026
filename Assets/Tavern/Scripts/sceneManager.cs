using UnityEngine;

public class sceneManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject mainCamera;
    public GameObject slotMachinePrefab;
    public GameObject updgradeBenchPrefab;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        testInputs();
    }
    void testInputs()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            OpenSlotMachineView();
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            OpenUpgradeView();
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            BackToRoom();
        }
    }
    public void OpenUpgradeView()
    {
        mainCamera.SetActive(false);
        updgradeBenchPrefab.SetActive(true);
        slotMachinePrefab.SetActive(false);
    }
    public void OpenSlotMachineView()
    {
        mainCamera.SetActive(false);
        updgradeBenchPrefab.SetActive(false);
        slotMachinePrefab.SetActive(true);
    }
    public void BackToRoom()
    {
        mainCamera.SetActive(true);
        updgradeBenchPrefab.SetActive(false);
        slotMachinePrefab.SetActive(false);
    }
}
