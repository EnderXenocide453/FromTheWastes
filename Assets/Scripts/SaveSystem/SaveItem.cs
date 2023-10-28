using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SaveItem : MonoBehaviour
{
    public int saveID;

    public abstract SaveInfo SaveInfo { get; }

    protected virtual void Start()
    {
        SaveItemCollector.AddSaveItem(this);
    }

    public abstract void Load(SaveInfo info);
}

[System.Serializable]
public class SaveInfo
{
    public string prefabPath;
    [SerializeField] public Vector3 position;
    public Quaternion rotation;

    public int[] upgraderInfo;
    public Dictionary<ResourceType, int>[] storagesInfo;
}
