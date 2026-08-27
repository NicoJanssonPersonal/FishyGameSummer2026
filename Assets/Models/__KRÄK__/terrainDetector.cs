using UnityEngine;

public class TerrainDetector : MonoBehaviour
{
    public Transform sharkParent; 
    
    public LayerMask terrainLayer;
    
    public float avoidanceSpeed = 4f;

    void Start()
    {
        if (sharkParent == null) sharkParent = transform.parent;
    }

    void OnTriggerStay(Collider other)
    {
        if (((1 << other.gameObject.layer) & terrainLayer) != 0)
        {
            Vector3 closestPoint = other.ClosestPointOnBounds(transform.position);

            Vector3 avoidDirection = transform.position - closestPoint;

            if (avoidDirection == Vector3.zero) 
                avoidDirection = sharkParent.up; 

            Quaternion targetRotation = Quaternion.LookRotation(avoidDirection.normalized);
            sharkParent.rotation = Quaternion.Slerp(
                sharkParent.rotation, 
                targetRotation, 
                Time.deltaTime * avoidanceSpeed
            );
        }
    }
}