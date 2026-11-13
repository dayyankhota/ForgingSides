using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MoveToSpot : MonoBehaviour
{
    public float manualStoppingDistance = 1f;
    public float delayBeforeTurn = 1.5f;
    public float idleSpinSpeed = 50f;
    public float turnAngle = 55f;
    public float turnSpeed = 90f;

    [Tooltip("If true, the NPC only counts as 'arrived' after it has finished turning to face the desk. " +
             "If false, it counts as arrived the moment it stops walking.")]
    public bool arriveAfterTurn = false;

    public bool HasArrived { get; private set; }

    private NavMeshAgent agent;
    private Transform targetTransform;
    private Animator animator;
    private bool hasStopped = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        GameObject targetObject = GameObject.FindGameObjectWithTag("Destination");

        if (agent == null || !agent.isOnNavMesh || targetObject == null)
        {
            Debug.LogWarning("[MoveToSpot] '" + name + "' can't walk to the desk (missing NavMeshAgent, NavMesh or 'Destination' tag). Starting order immediately.");
            HasArrived = true;
            return;
        }

        agent.autoBraking = false;
        agent.stoppingDistance = 0f;

        targetTransform = targetObject.transform;
        agent.SetDestination(targetTransform.position);
        if (animator != null) animator.SetBool("isWalking", true);
    }

    void Update()
    {
        if (hasStopped || targetTransform == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, targetTransform.position);
        if (distanceToTarget > manualStoppingDistance) return;

        hasStopped = true;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.updateRotation = false;
        if (animator != null) animator.SetBool("isWalking", false);

        if (!arriveAfterTurn) HasArrived = true;

        StartCoroutine(RotateLeft());
    }

    IEnumerator RotateLeft()
    {
        yield return new WaitForSeconds(delayBeforeTurn);

        Quaternion targetRotation = transform.rotation * Quaternion.Euler(0f, turnAngle, 0f);
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            yield return null;
        }
        transform.rotation = targetRotation;

        HasArrived = true;
    }
}