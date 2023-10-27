using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SaveItem
{
    public int saveID;

    public abstract SaveInfo SaveInfo { get; }

    public void Init()
    {
        SaveItemCollector.AddSaveItem(this);
    }

    public abstract void Load(SaveInfo info);
}

[System.Serializable]
public struct SaveInfo
{
    public string prefabPath;
    public Vector3 position;
    public Quaternion rotation;

    public Upgrader upgrader;
}
