using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Хранилище ресурсов
/// </summary>
public class Storage : MonoBehaviour
{
    public bool handleEverything;
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
    [SerializeField] private int _count = 0;

    private float _capacityMultiplier = 1;

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

    public event StorageEventHandler onSendEnds;

    public int Capacity
    {
        get => Mathf.RoundToInt(_capacityMultiplier * capacity);
    }

    public Dictionary<ResourceType, int> ResourcesCount { 
        get
        {
            if (_resources == null)
                return null;

            Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>();
            foreach (var res in _resources)
                resources.Add(res.Key, res.Value.count);

            return resources;
        }
    }

    private void Awake()
    {
        InitStorage();
    }

    /// <summary>
    /// Отправляет ресурс type в хранилище other в количестве count
    /// </summary>
    /// <param name="other">Хранилище-адресат</param>
    /// <param name="type">Тип отправляемого ресурса</param>
    /// <param name="sendCount">Количество отправляемого ресурса. Должно быть больше 0</param>
    /// <returns>Количество отправленного ресурса</return
    public int SendResource(Storage other, ResourceType type, int sendCount = 1)
    {
        //Проверка возможности обращения к ресурсу и наличия свободного места
        if (!_resources.ContainsKey(type) || !other._resources.ContainsKey(type) || other.filled) {
            onSendEnds?.Invoke(null);
            return 0;
        }

        //Если ресурсы бесконечны, оставляем количество без изменений.
        //Иначе, если ресурс есть в хранилище, ограничиваем его максимальным значением. Если ресурса нет, отправка не осуществляется
        sendCount = endless ? sendCount : (_resources[type].count > 0 ? Mathf.Clamp(sendCount, 1, _resources[type].count) : 0);

        //Если количество меньше 1, отправка невозможна
        if (sendCount < 1) {
            other.onSendEnds?.Invoke(null);
            onSendEnds?.Invoke(null);
            return 0;
        }

        AnimateSend(other.transform, _resources[type]);

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

    public void SetCapacityMultiplier(float multiplier)
    {
        _capacityMultiplier = multiplier;

        UpdateCounter();
    }

    /// <summary>
    /// Копирует свойства другого хранилища
    /// </summary>
    /// <param name="other">Объект копирования</param>
    public void SetCount(Dictionary<ResourceType, int> count)
    {
        InitStorage();

        foreach (var res in count)
            ChangeResource(res.Key, res.Value);
    }

    /// <summary>
    /// Метод инициализации хранилища
    /// </summary>
    private void InitStorage()
    {
        _resources = new Dictionary<ResourceType, Resource>();
        _count = 0;

        if (handleEverything) {
            resourceTypes = new ResourceType[ResourcesCollection.Resources.Count];
            int id = 0;

            foreach (var res in ResourcesCollection.Resources) {
                resourceTypes[id] = res.Key;

                id++;
            }
        } else if (resourceTypes == null || resourceTypes.Length == 0) {
            Debug.Log($"Не заполнен массив ресурсов в хранилище объекта {gameObject.name}");
            return;
        }

        foreach (var type in resourceTypes) {
            if (!_resources.TryAdd(type, ResourcesCollection.GetResource(type)))
                Debug.Log($"Ресурс типа {_resources[type].name} уже назначен в хранилище объекта {gameObject.name}");
            UpdateCounter();
        }
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
        if (verified == 0) {
            onSendEnds?.Invoke(null);
            return 0;
        }

        _count += verified;
        _resources[type].count += verified;

        CheckCount();
        UpdateUI(type);

        onCountChanged?.Invoke((type, count));

        return verified;
    }

    /// <summary>
    /// Проверка количества для вызова событий
    /// </summary>
    private void CheckCount()
    {
        if (endless)
            return;

        filled = false;
        if (_count == Capacity) {
            filled = true;
            onStorageFilled?.Invoke(null);
        } else if (_count == 0)
            onStorageEmptied?.Invoke(null);
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
            return 0;
        }

        //Если после добавления общее количество превысит вместительность, возвращаем разность вместительности и текущего общего количества
        if (_count + count > Capacity) {
            return Capacity - _count;
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
        UpdateCounter();
        resourceDisplay?.SetResource(type, GetResourceCount(type));
    }

    private void UpdateCounter()
    {
        if (counter)
            counter.text = $"{_count}/{Capacity}";
    }

    private void AnimateSend(Transform target, Resource sended)
    {
        ResourceAnimation res = Instantiate(Resources.Load<GameObject>(sended.prefabPath), transform.position, Quaternion.identity).GetComponent<ResourceAnimation>();
        res.StartFollow(target);
    }
}