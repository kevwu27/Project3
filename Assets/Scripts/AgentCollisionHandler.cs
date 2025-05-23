using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentCollisionHandler : MonoBehaviour
{
    public Transform agent;
    public AgentMovementTrigger movementScript;
    private Animator agentAnimator;

    void Start()
    {
        agentAnimator = agent.GetComponent<Animator>();
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Only trigger fall if we hit a wall, not the floor
        if (hit.collider.CompareTag("Wall"))
        {
            Debug.Log("Controller Hit: " + hit.collider.name);
            movementScript.StopMoving();
            agentAnimator.SetBool("Run", false);
            agentAnimator.SetTrigger("Fall");
        }
    }
}
