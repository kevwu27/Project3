using System;
using System.Collections.Generic;
using UnityEngine;

public class AnchorLoader : MonoBehaviour
{
    [SerializeField]
    private OVRSpatialAnchor defaultAnchor;
    private Action<bool, OVRSpatialAnchor.UnboundAnchor> _onLocalized;

    private int _playerUuidCount;

    void Start()
    {
        _onLocalized = OnLocalized;
    }

    public void LoadAnchorsByUuid()
    {
        if (!PlayerPrefs.HasKey(SpatialAnchorManager.NumUuidsPlayerPref))
        {
            PlayerPrefs.SetInt(SpatialAnchorManager.NumUuidsPlayerPref, 0);
        }

        _playerUuidCount = PlayerPrefs.GetInt(SpatialAnchorManager.NumUuidsPlayerPref);
        if (_playerUuidCount == 0)
        {
            Debug.Log("No anchors to load");
            return;
        }

        var uuids = new Guid[_playerUuidCount];
        for (int i = 0; i < _playerUuidCount; i++)
        {
            var playerPrefs = PlayerPrefs.GetString("uuid" + i);
            var saveData = SpatialAnchorSaveData.CreateFromString(playerPrefs);
            uuids[i] = saveData.AnchorUuid;

            Destroy(saveData.gameObject);
        }
        Load(uuids);
    }

    private async void Load(IEnumerable<Guid> uuids)
    {
        var unboundAnchors = new List<OVRSpatialAnchor.UnboundAnchor>();
        var result = await OVRSpatialAnchor.LoadUnboundAnchorsAsync(uuids, unboundAnchors);

        if (result.Success)
        {
            Debug.Log("Anchors loaded successfully.");

            foreach (var anchor in result.Value)
            {
                anchor.LocalizeAsync().ContinueWith(_onLocalized, anchor);
            }
        }
        else
        {
            Debug.LogError($"Failed to load anchors: {result.Status}");
        }
    }

    private void OnLocalized(bool success, OVRSpatialAnchor.UnboundAnchor unboundAnchor)
    {
        if (!success) return;

        string name = GetAnchorNameByUuid(unboundAnchor.Uuid);
        GameObject getAnchor = GetAnchorByName(name);
        OVRSpatialAnchor anchorPrefab;

        if (getAnchor != null)
        {
            anchorPrefab = getAnchor.GetComponent<OVRSpatialAnchor>();
        }
        else
        {
            anchorPrefab = defaultAnchor;
        }

        if (unboundAnchor.TryGetPose(out Pose pose))
        {
            OVRSpatialAnchor spatialAnchor = Instantiate(anchorPrefab, pose.position, pose.rotation);

            unboundAnchor.BindTo(spatialAnchor);
            Debug.Log("Localized anchor with UUID: " + spatialAnchor.Uuid + " and name: " + name);

            SpatialAnchorManager.instance.anchors.Add(spatialAnchor);
        }
        else
        {
            Debug.LogError("Failed to get pose for unbound anchor with UUID: " + unboundAnchor.Uuid);
        }
    }

    private GameObject GetAnchorByName(string name)
    {
        foreach (var anchor in SpatialAnchorManager.instance.SpatialAnchorPrefabs)
        {
            var anchorData = anchor.GetComponent<SpatialAnchorSaveData>();
            if (anchorData.Name.Contains(name))
            {
                return anchor;
            }
        }
        return null;
    }

    private string GetAnchorNameByUuid(Guid anchorUuid)
    {
        string name;
        for (int i = 0; i < _playerUuidCount; i++)
        {
            var anchorData = PlayerPrefs.GetString("uuid" + i);
            var saveData = SpatialAnchorSaveData.CreateFromString(anchorData);
            if (saveData.AnchorUuid == anchorUuid)
            {
                name = saveData.Name;
                Destroy(saveData.gameObject);
                return name;
            }
        }
        return null;
    }
}