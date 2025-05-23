using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentMovementTrigger : MonoBehaviour
{
    public Transform agent;
    public Transform handTarget; // RIGHT HAND
    public float travelSpeed = 0.1f;

    private bool shouldMove = false;
    private Animator agentAnimator;
    private CharacterController agentController;

    void Start()
    {
        agentAnimator = agent.GetComponent<Animator>();
        agentController = agent.GetComponent<CharacterController>();

        if (agentAnimator == null) Debug.LogError("Animator not found on agent.");
        if (agentController == null) Debug.LogError("CharacterController not found on agent.");
    }

    void Update()
    {
        if (!shouldMove || agent == null || handTarget == null) return;

        // Shouldn't move if animation is falling and getting up
        AnimatorStateInfo animState = agentAnimator.GetCurrentAnimatorStateInfo(0);
        if (animState.IsName("Fall") || animState.IsName("GetUp"))
            return;

        // Since we are not returning from above case, make sure animation state is running
        animState = agentAnimator.GetCurrentAnimatorStateInfo(0);
        if (shouldMove && animState.IsName("Running"))
        {
            // Move in the agent's current forward direction
            Vector3 direction = agent.forward;
            direction.y = 0;
            direction.Normalize();

            // Gravity
            Vector3 gravity = Vector3.down * 9.81f * Time.deltaTime;
            agentController.Move(gravity);


            // Physics based motion
            agentController.Move(direction * travelSpeed * Time.deltaTime);
        }
    }

    public void StartMoving()
    {
        Debug.Log("Right Gesture ON → Agent starts running.");
        shouldMove = true;
        if (agentAnimator != null)
            agentAnimator.SetBool("Run", true);
    }

    public void StopMoving()
    {
        Debug.Log("Right Gesture OFF → Agent stops.");
        shouldMove = false;
        if (agentAnimator != null)
            agentAnimator.SetBool("Run", false);
    }
}
