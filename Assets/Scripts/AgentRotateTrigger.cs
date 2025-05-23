using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimmyRotateTrigger : MonoBehaviour
{
    public Transform agent;
    public Transform handTarget; // LEFT HAND
    public float rotationSpeed = 10f;

    private bool shouldRotate = false;
    private Quaternion baseAgentRotation;
    private Quaternion baseHandRotation;

    void Start()
    {
        if (agent == null) Debug.LogError("Agent not assigned.");
        if (handTarget == null) Debug.LogError("Hand target not assigned.");
    }

    void Update()
    {
        if (!shouldRotate || agent == null || handTarget == null) return;

        // Get the rotation delta from hand
        Quaternion handDelta = Quaternion.Inverse(baseHandRotation) * handTarget.rotation;

        // Apply hand's horizontal rotation delta to Timmy
        Vector3 handEuler = handDelta.eulerAngles;
        float yaw = handEuler.y;

        Quaternion targetRotation = baseAgentRotation * Quaternion.Euler(0, yaw, 0);
        agent.rotation = Quaternion.Slerp(agent.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    public void StartRotating()
    {
        Debug.Log("Left Gesture ON → Unlock Rotation");
        shouldRotate = true;

        baseHandRotation = handTarget.rotation;
        baseAgentRotation = agent.rotation;
    }

    public void StopRotating()
    {
        Debug.Log("Left Gesture OFF → Lock Rotation");
        shouldRotate = false;
        baseAgentRotation = agent.rotation; // Save this as the new base
    }
}