using System.Collections;
using UnityEngine;

public class FishingController : MonoBehaviour
{
    public Rigidbody rb;
    public GameObject plop;
    Vector3 boatPos;
    [Header("Spawn Settings")]
    public float minRadius = 5;
    public float maxRadius = 15;
    public float spawnIntervall = 2;


    void Start()
    {
        boatPos = rb.position;
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            float randomDelay = Random.Range(spawnIntervall, spawnIntervall + 1);
            yield return new WaitForSeconds(randomDelay);
            
            boatPos = rb.position;
            
            SpawnPlop(minRadius, maxRadius); 
        }
    }

    void SpawnPlop(float minSpawnRadius, float maxSpawnRadius)
    {
        float randomAngle = Random.Range(0f, Mathf.PI * 2f);
        float randomDistance = Random.Range(minSpawnRadius, maxSpawnRadius);
        float xOffset = Mathf.Cos(randomAngle) * randomDistance;
        float zOffset = Mathf.Sin(randomAngle) * randomDistance;
        
        Vector3 spawnOffset = new Vector3(xOffset, 0.1f, zOffset);
        
        Vector3 spawnLocation = boatPos + spawnOffset;
       
        GameObject spawnedPlop = Instantiate(plop, spawnLocation, Quaternion.identity);

        Destroy(spawnedPlop, 5f);
    }
}