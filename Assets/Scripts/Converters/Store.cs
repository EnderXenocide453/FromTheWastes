using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Storage))]
public class Store : MonoBehaviour
{
    [SerializeField] private Storage importStorage;
    [SerializeField] private Converter.ConvertInfo[] priceList;

    private Storage _storeStorage;
    private Dictionary<ResourceType, int> _priceList;
    private GlobalValuesHandler _globalValues;

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
            Debug.Log("����� �� ������ ������� ���������");
            enabled = false;
        }
        if (priceList == null || priceList.Length == 0) {
            Debug.Log("����� �� ����� ������ ���");
            enabled = false;
        }

        foreach (var price in priceList) {
            _priceList.TryAdd(price.type, price.convertAmount);
        }

        importStorage.onCountChanged += (object obj) =>
        {
            (ResourceType type, int count) = ((ResourceType, int))obj;
            SellItem(type, count);
        };
    }

    private void SellItem(ResourceType type, int count)
    {
        //���� �������������� ����������� ������� �� ���������� ���������, ������ �� ������
        if (count < 0)
            return;
        if (!_priceList.ContainsKey(type)) {
            Debug.Log("������ �� ����� ���� ������! ���������� �������� ��������� �������� ��������� ��� ���!");
            return;
        }

        int price = _priceList[type] * count;

        importStorage.SendResource(_storeStorage, type, count);
        _globalValues.Cash += price;
    }
}