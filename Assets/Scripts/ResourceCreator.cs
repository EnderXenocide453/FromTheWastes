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
    public ResourceType type;
    public string prefabPath;
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
    public static Dictionary<ResourceType, Resource> resources { get; private set; }

    /// <summary>
    /// ����� ���������� ������� ��������
    /// </summary>
    /// <param name="resArray">������ ��������</param>
    public static void FillCollection(Resource[] resArray)
    {
        resources = new Dictionary<ResourceType, Resource>();

        foreach (var res in resArray) {
            if (!resources.TryAdd(res.type, res))
                Debug.Log($"������ {res.type} ��� ����������!");
        }
    }
}