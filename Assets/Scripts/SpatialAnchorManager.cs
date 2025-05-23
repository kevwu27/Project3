using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class SpatialAnchorManager : MonoBehaviour
{
    public static SpatialAnchorManager instance;
    public GameObject previewPrefab;        // Wall/quad preview
    public GameObject anchoredPrefab;       // Final anchor with OVRSpatialAnchor + SpatialAnchorSaveData
    public float distanceSpeed = 0.5f;      // Joystick sensitivity
    public float minDistance = 0.5f;
    public float maxDistance = 5f;
    public const string NumUuidsPlayerPref = "numUuids";
    public List<OVRSpatialAnchor> anchors = new();
    public OVRSpatialAnchor lastCreatedAnchor;
    public List<GameObject> SpatialAnchorPrefabs;

    private GameObject previewInstance;
    private float currentDistance = 1.5f;

    private void Start()
    {
        previewInstance = Instantiate(previewPrefab);
        previewInstance.SetActive(false);  // Only show when controller is tracked
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple SpatialAnchorManager instances in scene.");
        }
    }


    void Update()
    {
        // Show preview only when controller is tracked
        bool rightControllerTracked = OVRInput.IsControllerConnected(OVRInput.Controller.RTouch) &&
                                      OVRInput.GetControllerPositionTracked(OVRInput.Controller.RTouch);

        previewInstance.SetActive(rightControllerTracked);
        if (!rightControllerTracked) return;

        // Joystick Y axis to control distance
        float thumbY = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch).y;
        currentDistance += thumbY * distanceSpeed * Time.deltaTime;
        currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);

        // Position the preview
        Vector3 controllerPos = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
        Quaternion controllerRot = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);

        Vector3 forward = controllerRot * Vector3.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 previewPos = controllerPos + forward * currentDistance;
        previewPos.y = 5.0f;

        previewInstance.transform.position = previewPos;
        previewInstance.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);

        // Place anchor on trigger press
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
        {
            GameObject placed = Instantiate(anchoredPrefab, previewPos, Quaternion.LookRotation(forward, Vector3.up));
            placed.AddComponent<OVRSpatialAnchor>();

            var saveData = placed.AddComponent<SpatialAnchorSaveData>();
            saveData.Name = "Anchor" + DateTime.Now.ToString("yyyyMMdd_HHmmss");

            anchors.Add(placed.GetComponent<OVRSpatialAnchor>());
            lastCreatedAnchor = placed.GetComponent<OVRSpatialAnchor>();

        }

        // Save all anchors on A button press
        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
        {
            if (lastCreatedAnchor != null)
            {
                Debug.Log("Saved Last Anchor");
                SaveLastCreatedAnchor();
            }
            else
                Debug.LogWarning("No anchor to save.");
        }

        // Delete last anchor on B button press
        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch))
        {
            
            if (lastCreatedAnchor != null)
            {
                Debug.Log("Deleted Previous Anchor");
                UnSaveLastCreatedAnchor();
            }
            else
                Debug.LogWarning("No anchor to unsave.");
        }

        // Delete all on left B button press
        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.LTouch))
        {
            Debug.Log("Left Controller B → Unsave ALL anchors");
            UnsaveAllAnchors();
        }

    }

    public async void SaveLastCreatedAnchor()
    {
        if (lastCreatedAnchor == null)
        {
            Debug.Log("No anchor to save");
            return;
        }
        var result = await lastCreatedAnchor.SaveAnchorAsync();

        if (result.Success)
        {
            Debug.Log("Saved anchor with UUID: " + lastCreatedAnchor.Uuid);
            var saveData = lastCreatedAnchor.GetComponent<SpatialAnchorSaveData>();
            SaveUuidToPlayerPrefs(saveData);
        }
        else
        {
            Debug.Log("Failed to save anchor");
        }
    }

    public async void UnSaveLastCreatedAnchor()
    {
        if (lastCreatedAnchor == null)
        {
            Debug.Log("No anchor to unsave");
            return;
        }

        var result = await lastCreatedAnchor.EraseAnchorAsync();

        if (result.Success)
        {
            Debug.Log("Unsaved anchor with UUID: " + lastCreatedAnchor.Uuid);
            anchors.Remove(lastCreatedAnchor);
            Destroy(lastCreatedAnchor.gameObject);
        }
        else
        {
            Debug.Log("Failed to unsave anchor");
        }
    }

    private void SaveUuidToPlayerPrefs(SpatialAnchorSaveData data)
    {
        if (!PlayerPrefs.HasKey(NumUuidsPlayerPref))
        {
            PlayerPrefs.SetInt(NumUuidsPlayerPref, 0);
            Debug.Log("Save: NumUuidsPlayerPref not found, creating new one");
        }

        int playerNumUuids = PlayerPrefs.GetInt(NumUuidsPlayerPref);
        PlayerPrefs.SetString("uuid" + playerNumUuids, data.ToString());
        Debug.Log("Saved UUID to player prefs: " + data.ToString() + " with key: " + "uuid" + playerNumUuids);
        PlayerPrefs.SetInt(NumUuidsPlayerPref, ++playerNumUuids);
    }

    public void UnsaveAllAnchors()
    {
        foreach (var anchor in anchors)
        {
            if (anchor == null) continue;
            _ = anchor.EraseAnchorAsync(); // optional: await if you want to track success
            Destroy(anchor.gameObject);
        }

        anchors.Clear();
        ClearAllUuidsFromPlayerPrefs();
        Debug.Log("✅ All anchors unsaved and cleared.");
    }

    private void ClearAllUuidsFromPlayerPrefs()
    {
        if (PlayerPrefs.HasKey(NumUuidsPlayerPref))
        {
            int count = PlayerPrefs.GetInt(NumUuidsPlayerPref);
            for (int i = 0; i < count; i++)
            {
                PlayerPrefs.DeleteKey("uuid" + i);
            }

            PlayerPrefs.DeleteKey(NumUuidsPlayerPref);
            PlayerPrefs.Save();
        }
    }


}
