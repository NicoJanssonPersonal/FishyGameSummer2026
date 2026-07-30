using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFishSpawner : MonoBehaviour
{
    public GameObject SharkPrefab;
    public GameObject waterPlane;
    private MeshRenderer planeRenderer;

    [Header("Spawn Settings")]
    public int totalSharks = 3;
    public float minDistanceBetweenSharks = 5f;
    public int maxAttemptsPerShark = 100; // Prevents infinite loops if the plane is too crowded

    private List<Vector3> spawnedPositions = new List<Vector3>();

    void Start()
    {
        planeRenderer = waterPlane.GetComponent<MeshRenderer>();
        SpawnSharks();
    }

    void SpawnSharks()
    {
        for (int i = 0; i < totalSharks; i++)
        {
            Vector3 spawnPos = GetValidSpawnPosition();
            Instantiate(SharkPrefab, spawnPos, Quaternion.identity);
            spawnedPositions.Add(spawnPos);
        }
    }

    Vector3 GetValidSpawnPosition()
    {
        Vector3 candidatePosition;
        int attempts = 0;
        bool isValidPosition;

        do
        {
            candidatePosition = GetRandomPointOnThisPlane();
            isValidPosition = true;

            foreach (Vector3 existingPos in spawnedPositions)
            {
                if (Vector3.Distance(candidatePosition, existingPos) < minDistanceBetweenSharks)
                {
                    isValidPosition = false;
                    break;
                }
            }

            attempts++;

        } while (!isValidPosition && attempts < maxAttemptsPerShark);

        if (attempts >= maxAttemptsPerShark)
        {
            Debug.LogWarning("Could not find a valid spawn position far enough from other sharks within the attempt limit.");
        }

        return candidatePosition;
    }

    Vector3 GetRandomPointOnThisPlane()
    {
        Bounds bounds = planeRenderer.bounds;

        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomZ = Random.Range(bounds.min.z, bounds.max.z);

        float yPos = -1.15f;

        return new Vector3(randomX, yPos, randomZ);
    }
}