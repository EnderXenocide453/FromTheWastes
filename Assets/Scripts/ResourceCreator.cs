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
    public string prefabPath = "Prefabs/default.prefab";
    public string name = "unknown";
    public int count = 0;

    public Resource()
    {
        type = ResourceType.Waste;
        prefabPath = "Prefabs/default.prefab";
        name = "unknown";
        count = 0;
    }

    public Resource(Resource copy)
    {
        type = copy.type;
        prefabPath = copy.prefabPath;
        name = copy.name;
        count = copy.count;
    }

    public Resource(ResourceType type, string prefabPath, string name, int count)
    {
        this.type = type;
        this.prefabPath = prefabPath;
        this.name = name;
        this.count = count;
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
    Metal,
    Plastmass,
    Glass
}

/// <summary>
/// ��������� ����� ��� ������������������ �������� ����������� ��������
/// </summary>
public static class ResourcesCollection
{
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