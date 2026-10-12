using UnityEngine;
using UnityEngine.AI;

public class bertController : MonoBehaviour
{
    public Animator bertAnimator;
    public Transform targetPlane;

    private NavMeshAgent agentBert;

    void Start()
    {
        agentBert = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // Check if agent is close to destination or doesn't have an active path
        if (!agentBert.pathPending && agentBert.remainingDistance <= agentBert.stoppingDistance)
        {
            Vector3 point;
            if (RandomPointOnPlane(targetPlane, out point))
            {
                Debug.DrawRay(point, Vector3.up, Color.blue, 1.0f);
                agentBert.SetDestination(point);
            }
        }
    }

    bool RandomPointOnPlane(Transform plane, out Vector3 result)
    {
        if (plane == null)
        {
            result = Vector3.zero;
            return false;
        }

        // Standard Unity plane dimensions are 10 x 10 units
        float planeWidth = plane.localScale.x * 10f;
        float planeLength = plane.localScale.z * 10f;

        // Generate a random local point within plane boundaries (-half to +half)
        float randomX = Random.Range(-planeWidth / 2f, planeWidth / 2f);
        float randomZ = Random.Range(-planeLength / 2f, planeLength / 2f);

        // Convert offset point into world space position relative to the plane center
        Vector3 randomPoint = plane.position + (plane.right * randomX) + (plane.forward * randomZ);

        NavMeshHit hit;
        // Search distance updated to 5.0f to accommodate larger mesh scales safely
        if (NavMesh.SamplePosition(randomPoint, out hit, 5.0f, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }

        result = Vector3.zero;
        return false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            BertAttack();
        }
    }

    private void BertAttack()
    {
        if (bertAnimator != null)
        {
            bertAnimator.SetTrigger("bertBite");
        }
    }
}