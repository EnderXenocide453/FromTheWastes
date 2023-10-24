using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Хранилище ресурсов
/// </summary>
public class Storage : MonoBehaviour
{
    public bool filled { get; private set; }

    /// <summary>
    /// Типы хранимых ресурсов
    /// </summary>
    [SerializeField] private ResourceType[] resourceTypes;
    /// <summary>
    /// Объем хранилища
    /// </summary>
    [SerializeField] private int capacity;
    /// <summary>
    /// Бесконечен ли ресурс
    /// </summary>
    [SerializeField] private bool endless;
    [SerializeField] private TMP_Text counter;
    [SerializeField] private ResourceDisplay resourceDisplay;

    /// <summary>
    /// Ресурсы в хранилище
    /// </summary>
    private Dictionary<ResourceType, Resource> _resources;
    /// <summary>
    /// Общее количество хранимых ресурсов
    /// </summary>
    private int _count = 0;

    /// <summary>
    /// Делегат для событий хранилища
    /// </summary>
    public delegate void StorageEventHandler(object obj);
    /// <summary>
    /// Событие изменения количества ресурсов
    /// </summary>
    public event StorageEventHandler onCountChanged;
    /// <summary>
    /// Событие переполнения хранилища
    /// </summary>
    public event StorageEventHandler onStorageFilled;
    /// <summary>
    /// Событие опустошения хранилища
    /// </summary>
    public event StorageEventHandler onStorageEmptied;

    private void Awake()
    {
        InitStorage();
        onCountChanged += (object obj) => {
            (ResourceType type, int count) = ((ResourceType, int)) obj;

            CheckCount(obj);
            UpdateUI(type);
        };
    }

    /// <summary>
    /// Отправляет ресурс type в хранилище other в количестве count
    /// </summary>
    /// <param name="other">Хранилище-адресат</param>
    /// <param name="type">Тип отправляемого ресурса</param>
    /// <param name="sendCount">Количество отправляемого ресурса. Должно быть больше 0</param>
    /// /// <returns>Количество отправленного ресурса</return
    public int SendResource(Storage other, ResourceType type, int sendCount = 1)
    {
        //Проверка возможности обращения к ресурсу
        if (!_resources.ContainsKey(type))
            return 0;

        //Если ресурсы бесконечны, оставляем количество без изменений.
        //Иначе, если ресурс есть в хранилище, ограничиваем его максимальным значением. Если ресурса нет, отправка не осуществляется
        sendCount = endless ? sendCount : (_resources[type].count > 0 ? Mathf.Clamp(sendCount, 1, _resources[type].count) : 0);

        //Если количество меньше 1, отправка невозможна
        if (sendCount < 1) 
            return 0;

        //Отправка ресурсов и изменение количества собстенных ресурсов на принятое хранилищем-адресатом
        return -ChangeResource(type, -other.ChangeResource(type, sendCount));
    }

    public int GetCount() => _count;

    public int GetResourceCount(ResourceType type)
    {
        if (_resources.TryGetValue(type, out var res))
            return res.count;

        return 0;
    }

    public ResourceType[] GetResourceTypes()
    {
        ResourceType[] res = new ResourceType[resourceTypes.Length];
        resourceTypes.CopyTo(res, 0);

        return res;
    }

    public ResourceType[] FindIdentity(Storage other)
    {
        (ResourceType[] min, ResourceType[] max) = resourceTypes.Length > other.resourceTypes.Length ? (other.resourceTypes, resourceTypes) : (resourceTypes, other.resourceTypes);

        List<ResourceType> types = new List<ResourceType>();
        foreach (var type in min) {
            for (int i = 0; i < max.Length; i++) {
                if (type == max[i])
                    types.Add(type);
            }
        }

        return types.ToArray();
    }

    /// <summary>
    /// Метод инициализации хранилища
    /// </summary>
    private void InitStorage()
    {
        _resources = new Dictionary<ResourceType, Resource>();

        if (resourceTypes == null || resourceTypes.Length == 0) {
            Debug.Log($"Не заполнен массив ресурсов в хранилище объекта {gameObject.name}");
            return;
        }

        foreach (var type in resourceTypes) {
            if (!_resources.TryAdd(type, ResourcesCollection.GetResource(type)))
                Debug.Log($"Ресурс типа {_resources[type].name} уже назначен в хранилище объекта {gameObject.name}");
            //UpdateUI(type);
        }
    }

    /// <summary>
    /// Проверка количества для вызова событий
    /// </summary>
    private void CheckCount(object obj = null)
    {
        filled = false;
        if (_count == capacity) {
            filled = true;
            onStorageFilled?.Invoke(obj);
        }
        else if (_count == 0)
            onStorageEmptied?.Invoke(obj);
    }

    /// <summary>
    /// Изменяет количество ресурса type на значение count
    /// </summary>
    /// <param name="type">Тип изменяемого ресурса</param>
    /// <param name="count">Число, на которое изменится количество ресурса</param>
    /// <returns>Значение, на которое поменялось количество</returns>
    private int ChangeResource(ResourceType type, int count)
    {
        //Если ресурс бесконечен, возвращаем число без изменений
        if (endless) return count;

        int verified = VerifyResourceChange(type, count);

        //Если ресурс не может быть передан, возвращаем 0
        if (verified == 0)
            return 0;

        _count += verified;
        _resources[type].count += verified;

        onCountChanged?.Invoke((type, verified));

        Debug.Log($"Количество ресурса {_resources[type].name} теперь равно {_resources[type].count}");

        return verified;
    }

    /// <summary>
    /// Метод проверки изменений ресурса
    /// </summary>
    /// <param name="type">Тип ресурса</param>
    /// <param name="count">Число, на которое изменяется количество ресурса</param>
    /// <returns>Число, на которое может измениться количество</returns>
    private int VerifyResourceChange(ResourceType type, int count)
    {
        //Проверка возможности хранения ресурса
        if (!_resources.ContainsKey(type)) {
            Debug.Log($"Ресурс {type} не может храниться в {gameObject.name}");

            return 0;
        }

        //Если после добавления общее количество превысит вместительность, возвращаем разность вместительности и текущего общего количества
        if (_count + count > capacity) {
            return capacity - _count;
        }

        //Если при добавлении числа к количеству ресурса оно станет отрицательным, возвращаем отрицательное количество ресурса
        if (_resources[type].count + count < 0) {
            return -_resources[type].count;
        }

        //Если выхода за рамки не последовало, возвращаем число неизменным
        return count;
    }

    private void UpdateUI(ResourceType type)
    {
        if (counter)
            counter.text = $"{_count}/{capacity}";

        resourceDisplay?.SetResource(type, GetResourceCount(type));
    }
}