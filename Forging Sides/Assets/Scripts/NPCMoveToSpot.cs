using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MoveToSpot : MonoBehaviour
{
    public float manualStoppingDistance = 1f;
    public float delayBeforeTurn = 1.5f;
    private NavMeshAgent agent;
    private Transform targetTransform;
     private Animator animator;
      public float idleSpinSpeed = 50f;
      private bool hasRotated = false;
      public float turnAngle = 55f;
      public float turnSpeed = 90f;

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

            if(!hasRotated)
            {
                hasRotated = true;
                agent.updateRotation = false;
                StartCoroutine(RotateLeft());
            }
 
        }
    }
     IEnumerator RotateLeft()
     {
         
          yield return new WaitForSeconds(delayBeforeTurn);
         Quaternion startRotation = transform.rotation;
         Quaternion targetRotation = startRotation * Quaternion.Euler(0f,turnAngle, 0f);
         while(Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
         {
             transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
             yield return null;
         }
         transform.rotation = targetRotation;
           Debug.Log("Rotation finished!");
     }
}