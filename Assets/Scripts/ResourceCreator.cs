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
    public ResourceType type;
    public string prefabPath;
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
    public static Dictionary<ResourceType, Resource> resources { get; private set; }

    /// <summary>
    /// Метод заполнения словаря ресурсов
    /// </summary>
    /// <param name="resArray">Массив ресурсов</param>
    public static void FillCollection(Resource[] resArray)
    {
        resources = new Dictionary<ResourceType, Resource>();

        foreach (var res in resArray) {
            if (!resources.TryAdd(res.type, res))
                Debug.Log($"Ресурс {res.type} уже существует!");
        }
    }
}