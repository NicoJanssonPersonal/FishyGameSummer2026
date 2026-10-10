using UnityEngine;
using UnityEngine.AI;

public class bertController : MonoBehaviour
{
    public Animator bertAnimator;
    [SerializeField] private float rotationSpeed = 5.0f;
    [SerializeField] private float stoppingDistance = 0.5f;

    public GameObject[] bertTargets;

    private NavMeshAgent agentBert;
    private Vector3 currentTarget;

    void Start()
    {
        agentBert = GetComponent<NavMeshAgent>();

        if (agentBert != null)
        {
            agentBert.updateRotation = false;
        }

        PickNewRandomTarget();
    }

    void Update()
    {
        if (agentBert == null || !agentBert.isActiveAndEnabled || !agentBert.isOnNavMesh) return;
        if (bertTargets == null || bertTargets.Length == 0) return;

        agentBert.SetDestination(currentTarget);

        RotateBertTowardsGoal();

        if (!agentBert.pathPending && agentBert.remainingDistance <= stoppingDistance)
        {
            PickNewRandomTarget();
        }
    }

    private void PickNewRandomTarget()
    {
        if (bertTargets != null && bertTargets.Length > 0)
        {
            int randomIndex = Random.Range(0, bertTargets.Length);

            if (bertTargets[randomIndex] != null)
            {
                currentTarget = bertTargets[randomIndex].transform.position;
            }
        }
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

    private void RotateBertTowardsGoal()
    {
        Vector3 direction = agentBert.velocity;

        if (direction.sqrMagnitude < 0.1f)
        {
            direction = currentTarget - transform.position;
        }

        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);

            targetRotation *= Quaternion.Euler(0, 180f, 0);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}