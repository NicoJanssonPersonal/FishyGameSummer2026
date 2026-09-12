using System.Collections;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{

    [Header("UI & Settings")]
    public GameObject mainMenuUI;
    public float transitionDuration = 2.5f;


    public GameObject UIManager;

    void Start()
    {
        UIManager.SetActive(false);
        Time.timeScale = 0f;
    }

    public void PlayGame()
    {
        if (mainMenuUI != null)
            mainMenuUI.SetActive(false);

        Time.timeScale = 1f;
        UIManager.SetActive(true);
    }
}