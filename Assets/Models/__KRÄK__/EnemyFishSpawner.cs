using UnityEngine;

public class EnemyFishSpawner : MonoBehaviour
{
    public GameObject SharkPrefab;
    public GameObject waterPlane;
    private MeshRenderer planeRenderer;

    void Start()
    {
        planeRenderer = waterPlane.GetComponent<MeshRenderer>();

        for (int i = 0; i < 3; i++)
        {
            Instantiate(SharkPrefab, GetRandomPointOnThisPlane(), Quaternion.identity);
        }
    }

    void Update()
    {
        
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
