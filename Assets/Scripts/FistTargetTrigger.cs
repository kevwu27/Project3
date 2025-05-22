using UnityEngine;
using UnityEngine.AI;

public class FistTargetTrigger : MonoBehaviour
{
    public Transform agent;
    public Transform handTarget; // where the hand (left/right) is located

    private bool shouldMove = false;
    private NavMeshAgent agentNav;
    private Animator agentAnimator;

    void Start()
    {
        agentNav = agent.GetComponent<NavMeshAgent>();
        agentAnimator = agent.GetComponent<Animator>();

        if (agentNav == null)
        {
            Debug.LogError("No NavMeshAgent found on agent.");
        }
    }

    void Update()
    {
        if (!shouldMove || agent == null || handTarget == null) return;

        if (agentAnimator != null)
        {
            float speed = agentNav.velocity.magnitude;
            agentAnimator.SetFloat("speed", speed);
        }

        if (shouldMove)
        {
            if (agentNav.isOnNavMesh)
            {
                agentNav.SetDestination(handTarget.position);
            }
        }
        else
        {
            if (!agentNav.pathPending && agentNav.hasPath)
            {
                agentNav.ResetPath();
            }
        }
    }

    public void StartMovingToHand()
    {
        Debug.Log("Fist recognized: start moving to hand.");
        shouldMove = true;
    }

    public void StopMoving()
    {
        Debug.Log("Fist released: stop moving.");
        shouldMove = false;
    }
}
