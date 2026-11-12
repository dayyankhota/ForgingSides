using UnityEngine;
using UnityEngine.AI;

public class MoveToSpot : MonoBehaviour
{
    public float manualStoppingDistance = 1f;

    private NavMeshAgent agent;
    private Transform targetTransform;
     private Animator animator;

    void Start()
    {
        GameObject targetObject = GameObject.FindGameObjectWithTag("Destination");
        agent = GetComponent<NavMeshAgent>();
        animator= GetComponent<Animator>();
        agent.autoBraking = false;

        if (agent != null)
        {
            agent.stoppingDistance = 0f;
        }

   
        if (agent != null && agent.isOnNavMesh && targetObject != null)
        {
            targetTransform = targetObject.transform;
            agent.SetDestination(targetTransform.position);
            animator.SetBool("isWalking", true);
        }
    }

    void Update()
    {
        if (targetTransform == null || agent == null) return;

  
        float distanceToTarget = Vector3.Distance(transform.position, targetTransform.position);

        if (distanceToTarget <= manualStoppingDistance)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            animator.SetBool("isWalking", false);
        }
    }
}