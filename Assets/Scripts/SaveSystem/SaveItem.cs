using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ����� ������� ����������
/// </summary>
public abstract class SaveItem : MonoBehaviour
{
    public string path;
    public int saveID;

    private SaveInfo _loadInfo;

    public abstract SaveInfo SaveInfo { get; }

    protected virtual void Start()
    {
        //����� ��������� ���������� � ������ �������� ���������� ����������
        SaveItemCollector.AddSaveItem(this);

        //���� ���� ��� ���������, ���������
        if (_loadInfo != null)
            Load(_loadInfo);
    }

    /// <summary>
    /// ���������� ���������� ��������
    /// </summary>
    /// <param name="info">��������� ������� ��� ��������</param>
    public void SetLoadInfo(SaveInfo info)
    {
        _loadInfo = info;
    }

    /// <summary>
    /// ��������� ���������
    /// </summary>
    /// <param name="info">���������</param>
    protected abstract void Load(SaveInfo info);
}

/// <summary>
/// ��������� ������� ����������
/// </summary>
[System.Serializable]
public class SaveInfo
{
    public string prefabPath;
    [SerializeField] public Vector3 position;
    public Quaternion rotation;

    public int[] upgraderInfo;
    public Dictionary<ResourceType, int>[] storagesInfo;
}
