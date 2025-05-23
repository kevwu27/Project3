using System;
using UnityEngine;

public class SpatialAnchorSaveData : MonoBehaviour
{
    public Guid AnchorUuid;
    public string Name;

    public override string ToString()
    {
        return $"{AnchorUuid}, {Name}";
    }

    public static SpatialAnchorSaveData CreateFromString(string data)
    {
        GameObject tempGameObject = new("TempSaveData");
        var saveData = tempGameObject.AddComponent<SpatialAnchorSaveData>();
        saveData.FromString(data);
        return saveData;
    }

    public SpatialAnchorSaveData FromString(string data)
    {
        var parts = data.Split(new[] { ", " }, StringSplitOptions.None);
        if (parts.Length != 2)
        {
            throw new FormatException("Invalid data format");
        }
        AnchorUuid = new Guid(parts[0]);
        Name = parts[1];
        return this;
    }
}