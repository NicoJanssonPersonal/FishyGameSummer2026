using UnityEngine;
using UnityEngine.SceneManagement;

public class sceneManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject mainCamera;
    public GameObject slotMachinePrefab;
    public GameObject updgradeBenchPrefab;

    public GameObject endRunOutline;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && mainCamera.gameObject.activeInHierarchy)
        {
            endRunOutline.SetActive(true);
            backOutofTavern();
        }
    }
    public void backOutofTavern()
    {
        SceneManager.LoadScene("Map");
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
