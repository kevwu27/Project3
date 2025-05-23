using UnityEngine;
using UnityEngine.AI;
using static OVRHand;

public class FistTargetTrigger : MonoBehaviour
{
    public Transform agent;
    public Transform handTarget; // where the hand (left/right) is located
    private Animator agentAnimator;

    public OVRHand hand;
    private bool previousPinchState = false;
    private bool isRunning = false;
    public float runSpeed = 0.1f;
    private Rigidbody rb;

    void Start()
    {
        agentAnimator = agent.GetComponent<Animator>();
        rb = agent.GetComponent<Rigidbody>();
    }

    void Update()
    {
        bool isIndexFingerPinching = hand.GetFingerIsPinching(HandFinger.Middle);

        // if (isIndexFingerPinching)
        // {
        //     bool isRunning = agentAnimator.GetBool("Run");
        //     agentAnimator.SetBool("Run", !isRunning);
        // }

        if (isIndexFingerPinching && !previousPinchState)
        {
            isRunning = !isRunning;
            agentAnimator.SetBool("Run", isRunning);
        }

        previousPinchState = isIndexFingerPinching;

        AnimatorStateInfo animState = agentAnimator.GetCurrentAnimatorStateInfo(0);
        // Move agent forward if running
        if (isRunning && animState.IsName("Running"))
        {
            Vector3 moveDirection = agent.forward * runSpeed * Time.deltaTime;
            rb.MovePosition(rb.position + moveDirection);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collided with " + collision.gameObject.name);
        // Stop moving or trigger animation change here
        isRunning = false;
        agentAnimator.SetBool("Run", false);
    }
}
