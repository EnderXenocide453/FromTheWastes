using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Класс создателя ресурсов. Передаёт заполненный массив ресурсов в статичный класс ResourcesCollection
/// </summary>
public class ResourceCreator : MonoBehaviour
{
    /// <summary>
    /// Список ресурсов
    /// </summary>
    [SerializeField] private Resource[] resources;

    private void Awake()
    {
        if (resources == null) {
            Debug.Log("Список ресурсов пуст!");
            
            return;
        }

        ResourcesCollection.FillCollection(resources);
    }
}

/// <summary>
/// Класс ресурса. Содержит свой тип и путь к префабу
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
/// Перечисление всех типов ресурсов
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
/// Статичный класс для структурированного хранения экземпляров ресурсов
/// </summary>
public static class ResourcesCollection
{
    /// <summary>
    /// Словарь с экземплярами ресурсов
    /// </summary>
    private static Dictionary<ResourceType, Resource> _resources;

    /// <summary>
    /// Метод заполнения словаря ресурсов
    /// </summary>
    /// <param name="resArray">Массив ресурсов</param>
    public static void FillCollection(Resource[] resArray)
    {
        _resources = new Dictionary<ResourceType, Resource>();

        foreach (var res in resArray) {
            if (!_resources.TryAdd(res.type, new Resource(res)))
                Debug.Log($"Ресурс {res.type} уже существует!");
        }
    }

    public static Resource GetResource(ResourceType type)
    {
        if (!_resources.ContainsKey(type))
            return new Resource();

        return new Resource(_resources[type]);
    }
}