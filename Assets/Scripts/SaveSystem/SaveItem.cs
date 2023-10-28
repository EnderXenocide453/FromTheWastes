using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SaveItem : MonoBehaviour
{
    //Для первого сохранения
    public string path;

    public int saveID;

    private SaveInfo _loadInfo;

    public abstract SaveInfo SaveInfo { get; }

    protected virtual void Start()
    {
        SaveItemCollector.AddSaveItem(this);
        Load(_loadInfo);
    }

    public void SetLoadInfo(SaveInfo info)
    {
        _loadInfo = info;
    }

    protected abstract void Load(SaveInfo info);
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
