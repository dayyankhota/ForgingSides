using UnityEngine;
using UnityEngine.AI;

public class MoveToSpot : MonoBehaviour
{
   
  

    private NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        GameObject targetObj = GameObject.FindWithTag("Destination");
    }

    public void SetDestination(Vector3 targetPosition)
    {
        if (agent != null)
        {
            agent.SetDestination(targetPosition);
        }
    }
}