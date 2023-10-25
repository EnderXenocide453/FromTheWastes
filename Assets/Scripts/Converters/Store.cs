using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Storage))]
public class Store : MonoBehaviour
{
    private GlobalValuesHandler _globalValues;
    private Dictionary<ResourceType, int> _priceList;
    private Storage _storeStorage;
    [SerializeField] private Storage importStorage;
    [SerializeField] private ConvertInfo[] priceList;
    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        _globalValues = GameObject.FindWithTag("Global")?.GetComponent<GlobalValuesHandler>();
        _storeStorage = GetComponent<Storage>();
        _priceList = new Dictionary<ResourceType, int>();

        if (!importStorage) {
            Debug.Log("Банку не задано входное хранилище");
            enabled = false;
        }
        if (priceList == null || priceList.Length == 0) {
            Debug.Log("Банку не задан список цен");
            enabled = false;
        }

        foreach (var price in priceList) {
            _priceList.TryAdd(price.type, price.amount);
        }

        importStorage.onCountChanged += (object obj) =>
        {
            (ResourceType type, int count) = ((ResourceType, int))obj;
            SellItem(type, count);
        };
    }

    private void SellItem(ResourceType type, int count)
    {
        //Если регистрируется перемещение ресурса во внутреннее хранилище, ничего не делаем
        if (count < 0)
            return;
        if (!_priceList.ContainsKey(type)) {
            Debug.Log("Ресурс не может быть продан! Необходимо изменить настройки входного хранилища или цен!");
            return;
        }

        int price = _priceList[type] * count;

        importStorage.SendResource(_storeStorage, type, count);
        _globalValues.Cash += price;
    }
}