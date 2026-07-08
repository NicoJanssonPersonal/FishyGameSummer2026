using System.Collections;
using UnityEngine;

public class PlopSpawner : MonoBehaviour
{
    public Rigidbody rb;
    public GameObject plop;
    Vector3 boatPos;

    void Start()
    {
        boatPos = rb.position;
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            float randomDelay = Random.Range(GlobalStats.spawnIntervall, GlobalStats.spawnIntervall + 1);
            
            int totalPlopsToSpawn = Mathf.RoundToInt(GlobalStats.plopAmount) * 5;
            
            yield return StartCoroutine(SpawnPlopRoutine(GlobalStats.minRadius, GlobalStats.maxRadius, totalPlopsToSpawn, randomDelay));
        }
    }
    IEnumerator SpawnPlopRoutine(float minSpawnRadius, float maxSpawnRadius, int totalAmount, float waveDuration)
    {
        float delayBetweenSpawns = waveDuration / totalAmount;

        for (int i = 0; i < totalAmount; i++)
        {
            boatPos = rb.position; 

            float randomAngle = Random.Range(0f, Mathf.PI * 2f);
            float randomDistance = Random.Range(minSpawnRadius, maxSpawnRadius);
            float xOffset = Mathf.Cos(randomAngle) * randomDistance;
            float zOffset = Mathf.Sin(randomAngle) * randomDistance;

            Vector3 spawnOffset = new Vector3(xOffset, 0.1f, zOffset);
            Vector3 spawnLocation = boatPos + spawnOffset;
            
            GameObject spawnedPlop = Instantiate(plop, spawnLocation, Quaternion.identity);
            StartCoroutine(ChangeAllChildrenOpacityRoutine(spawnedPlop));

            yield return new WaitForSeconds(delayBetweenSpawns);
        }
    }

    private IEnumerator ChangeAllChildrenOpacityRoutine(GameObject parentObj)
    {
        float currentTime = 0;
        float fadeDuration = 5f;

        Renderer[] Renderers = parentObj.GetComponentsInChildren<Renderer>();

        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;

            float alpha = Mathf.Lerp(1f, 0f, currentTime / fadeDuration);

            foreach (Renderer rend in Renderers)
            {
                if (rend != null)
                {
                    Color c = rend.material.color;
                    c.a = alpha;
                    rend.material.color = c;
                }
            }

            yield return null;
        }
        
        if (parentObj != null)
        {
            Destroy(parentObj);
        }
    }
}