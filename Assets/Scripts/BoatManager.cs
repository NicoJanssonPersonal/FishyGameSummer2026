using System.Collections;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BoatManager : MonoBehaviour
{
    public GameObject fishradius;
    private float fishrangeRatio = 1.4f;

    private Rigidbody rb;
    public Collider boatCollider;
    private bool isSinking = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    void Start()
    {
        
    }

    void Update()
    {
        updateFishingRange();
        checkHp();
    }
    void updateFishingRange()
    {
        float fishRange = GlobalStats.fishingRange * fishrangeRatio;
        fishradius.transform.localScale = new Vector3(fishRange, 1, fishRange);
    }
    void checkHp()
    {
        if (GlobalStats.currentHealth <= 0)
        {
            Debug.Log("game OVER");
            StartSinking();
            fishradius.SetActive(false);
        }
        if(transform.position.y <= -10)
        {
            //Time.timeScale = 0;
            //pause game popup window u died return to tavern
            StartCoroutine(changeScene("TAVERN"));
        }
    }
    public void StartSinking()
    {
        isSinking = true;
        
        if (boatCollider!= null)
        {
            boatCollider.enabled = false;
        }
    }

    void FixedUpdate()
    {
        if (isSinking)
        {
            Vector3 downwardMovement = Vector3.down * 2 * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + downwardMovement);
        }
    }
    private IEnumerator changeScene(string scene)
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(scene);

    }
}
