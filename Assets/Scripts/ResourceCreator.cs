using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ����� ��������� ��������. ������� ����������� ������ �������� � ��������� ����� ResourcesCollection
/// </summary>
public class ResourceCreator : MonoBehaviour
{
    /// <summary>
    /// ������ ��������
    /// </summary>
    [SerializeField] private Resource[] resources;

    private void Awake()
    {
        if (resources == null) {
            Debug.Log("������ �������� ����!");
            
            return;
        }

        ResourcesCollection.FillCollection(resources);
    }
}

/// <summary>
/// ����� �������. �������� ���� ��� � ���� � �������
/// </summary>
[System.Serializable]
public class Resource
{
    public ResourceType type = ResourceType.Waste;
    public string iconPath = "Icons/default";
    public string prefabPath = "Prefabs/default";
    public string name = "unknown";
    public int count = 0;

    public Resource()
    {
        type = ResourceType.Waste;
        iconPath = "Icons/default";
        prefabPath = "Prefabs/default";
        name = "unknown";
        count = 0;
    }

    public Resource(Resource copy)
    {
        type = copy.type;
        iconPath = copy.iconPath;
        prefabPath = copy.prefabPath;
        name = copy.name;
        count = 0;
    }

    public Resource(ResourceType type, string prefabPath, string iconPath, string name, int count)
    {
        this.type = type;
        this.iconPath = iconPath;
        this.prefabPath = prefabPath;
        this.name = name;
        this.count = 0;
    }
}

/// <summary>
/// ������������ ���� ����� ��������
/// </summary>
public enum ResourceType
{
    Waste,
    Garbage,
    UsefulJunk,
    Material,
    DamagedDetail,
    RestoredDetail,
    DamagedTool,
    RepairedTool,
    Cash
}

/// <summary>
/// ��������� ����� ��� ������������������ �������� ����������� ��������
/// </summary>
public static class ResourcesCollection
{
    public static Dictionary<ResourceType, Resource> Resources 
    { 
        get => new Dictionary<ResourceType, Resource>(_resources);
    }

    /// <summary>
    /// ������� � ������������ ��������
    /// </summary>
    private static Dictionary<ResourceType, Resource> _resources;

    /// <summary>
    /// ����� ���������� ������� ��������
    /// </summary>
    /// <param name="resArray">������ ��������</param>
    public static void FillCollection(Resource[] resArray)
    {
        _resources = new Dictionary<ResourceType, Resource>();

        foreach (var res in resArray) {
            if (!_resources.TryAdd(res.type, new Resource(res)))
                Debug.Log($"������ {res.type} ��� ����������!");
        }
    }

    public static Resource GetResource(ResourceType type)
    {
        if (!_resources.ContainsKey(type))
            return new Resource();

        return new Resource(_resources[type]);
    }
}