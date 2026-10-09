using UnityEngine;
using UnityEngine.AI;

public class bertController : MonoBehaviour
{
    public Animator bertAnimator;
    [SerializeField] private float rotationSpeed = 5.0f;

    private NavMeshAgent agentBert;
    private Transform targetTransform;

    void Start()
    {
        agentBert = GetComponent<NavMeshAgent>();

        if (agentBert != null)
        {
            agentBert.updateRotation = false;
        }

        GameObject targetObj = GameObject.FindGameObjectWithTag("BertTarget");
        if (targetObj != null)
        {
            targetTransform = targetObj.transform;
        }
        else
        {
            Debug.LogWarning("bertController: No GameObject found with tag 'BertTarget'!");
        }
    }

    void Update()
    {
        if (targetTransform == null) return;
        if (agentBert == null || !agentBert.isActiveAndEnabled || !agentBert.isOnNavMesh) return;

        agentBert.SetDestination(targetTransform.position);

        RotateBertTowardsGoal();
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

        if (direction.sqrMagnitude < 0.1f && targetTransform != null)
        {
            direction = targetTransform.position - transform.position;
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