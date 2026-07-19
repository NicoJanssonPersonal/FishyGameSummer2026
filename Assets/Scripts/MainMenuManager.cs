using UnityEngine;
using UnityEngine.SceneManagement; // Required for changing scenes

public class MainMenuManager : MonoBehaviour
{
    public GameObject mainMenuManager;
    public Camera maincamera;
    private Vector3 startCameraPos;
    void Start()
    {
        startCameraPos = maincamera.transform.position;
        Time.timeScale = 0;
        maincamera.transform.position = new Vector3(0,100,0);
    }
    public void PlayGame()
    {
        // Loads the next scene in your Build Settings queue. 
        // You can also use a scene name in quotes, like: SceneManager.LoadScene("GameScene");
        Debug.Log("Start menu opened!");
        maincamera.transform.position = startCameraPos;
        Time.timeScale = 1;
        mainMenuManager.SetActive(false);
    }

    public void OpenOptions()
    {
        // For now, this just prints to the console to prove it works.
        // Later, you can make this open an options panel.
        Debug.Log("Options menu opened!");
    }

    public void ExitGame()
    {
        Debug.Log("Game Exited!");
        Application.Quit(); // Closes the game (only works in a built application, not inside the Unity Editor)
    }
}