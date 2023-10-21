using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Хранилище ресурсов
/// </summary>
public class Storage : MonoBehaviour
{
    /// <summary>
    /// Типы хранимых ресурсов
    /// </summary>
    [SerializeField] private ResourceType[] resouceTypes;
    /// <summary>
    /// Объем хранилища
    /// </summary>
    [SerializeField] private int capacity;
    /// <summary>
    /// Бесконечен ли ресурс
    /// </summary>
    [SerializeField] private bool endless;

    /// <summary>
    /// Ресурсы в хранилище и их количество
    /// </summary>
    private Dictionary<ResourceType, int> _resources;
    /// <summary>
    /// Общее количество хранимых ресурсов
    /// </summary>
    private int _count = 0;

    /// <summary>
    /// Делегат для событий хранилища
    /// </summary>
    public delegate void StorageEventHandler();
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
        onCountChanged += CheckCount;
    }

    public int GetResourceCount(ResourceType type)
    {
        if (_resources.ContainsKey(type))
            return _resources[type];

        return 0;
    }

    /// <summary>
    /// Отправляет ресурс type в хранилище other в количестве count
    /// </summary>
    /// <param name="other">Хранилище-адресат</param>
    /// <param name="type">Тип отправляемого ресурса</param>
    /// <param name="sendCount">Количество отправляемого ресурса. Должно быть больше 0</param>
    public void SendResource(Storage other, ResourceType type, int sendCount = 1)
    {
        //Если количество меньше 1, отправка невозможна
        if (sendCount < 1) 
            return;
        //Проверка возможности обращения к ресурсу
        if (!_resources.ContainsKey(type))
            return;

        //Если ресурс не бесконечен и есть в хранилище
        if (_resources[type] > 0 && !endless)
            //Ограничение отправляемого количества пределами от 1 до количества ресурсов в хранилище
            sendCount = Mathf.Clamp(sendCount, 1, _resources[type]);

        //Отправка ресурсов и изменение количества собстенных ресурсов на принятое хранилищем-адресатом
        ChangeResource(type, -other.ChangeResource(type, sendCount));
    }

    /// <summary>
    /// Метод инициализации хранилища
    /// </summary>
    private void InitStorage()
    {
        _resources = new Dictionary<ResourceType, int>();

        if (resouceTypes == null || resouceTypes.Length == 0) {
            Debug.Log($"Не заполнен массив ресурсов в хранилище объекта {gameObject.name}");
            return;
        }

        foreach (var type in resouceTypes)
            if (!_resources.TryAdd(type, 0))
                Debug.Log($"Ресурс типа {type} уже назначен в хранилище объекта {gameObject.name}");
    }

    /// <summary>
    /// Проверка количества для вызова событий
    /// </summary>
    private void CheckCount()
    {
        if (_count == capacity)
            onStorageFilled?.Invoke();
        else if (_count == 0)
            onStorageEmptied?.Invoke();
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
        if (endless) return 0;

        int verified = VerifyResourceChange(type, count);

        //Если ресурс не может быть передан, возвращаем 0
        if (verified == 0)
            return 0;

        _count += verified;
        _resources[type] += verified;

        onCountChanged?.Invoke();

        Debug.Log($"Количество ресурса {type} теперь равно {_resources[type]}");

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
        if (_resources[type] + count < 0) {
            return -_resources[type];
        }

        //Если выхода за рамки не последовало, возвращаем число неизменным
        return count;
    }
}