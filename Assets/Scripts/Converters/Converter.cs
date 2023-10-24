using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Storage))]
public class Converter : MonoBehaviour
{
    [SerializeField] private string converterName = "��� �����";
    [SerializeField] private Storage[] importStorages;
    [SerializeField] private Storage[] exportStorages;
    [SerializeField] private ConvertInfo[] convertCost;
    [SerializeField] private ConvertInfo[] convertResult;
    [SerializeField] private float workAmount = 1;
    [SerializeField] private float workSpeed = 1;

    private Storage _converterStorage;
    private float _workProgress;

    private Dictionary<ResourceType, int> _readyResources;

    private bool _ready;
    private bool _started;

    public delegate void ConverterHandler();
    public event ConverterHandler onConvertEnds;
    public event ConverterHandler onConvertStart;
    public event ConverterHandler onConverterReady;

    private void Start()
    {
        ValidateResources();
        InitStorage();
    }

    private void FixedUpdate()
    {
        if (_started)
            Convert(Time.fixedDeltaTime);
    }

    private void ValidateResources()
    {
        if (importStorages == null || importStorages.Length == 0) {
            Debug.Log($"���� ����� �������� ��������������� {converterName} �� ���������!");
            enabled = false;
            
            return;
        } if (exportStorages == null || exportStorages.Length == 0) {
            Debug.Log($"���� ������ �������� ��������������� {converterName} �� ���������!");
            enabled = false;

            return;
        } if (convertCost == null) {
            Debug.Log($"��������� �������������� � {converterName} �� ���������!");
            enabled = false;

            return;
        } if (convertResult == null) {
            Debug.Log($"��������� �������������� � {converterName} �� ���������!");
            enabled = false;

            return;
        }
    }

    private void InitStorage()
    {
        _readyResources = new Dictionary<ResourceType, int>();
        _converterStorage = GetComponent<Storage>();

        foreach(var res in convertCost)
            _readyResources.TryAdd(res.type, 0);

        foreach (var storage in importStorages)
            storage.onCountChanged += (object obj) => HandleResourceChange(((ResourceType, int)) obj);
    }

    private void HandleResourceChange((ResourceType type, int count) change)
    {
        if (!_readyResources.ContainsKey(change.type))
            return;

        _readyResources[change.type] += change.count;
        CheckReadiness();
    }

    private void CheckReadiness()
    {
        foreach (var cost in convertCost) {
            if (_readyResources[cost.type] < cost.convertAmount) {
                _ready = false;
                return;
            }
        }

        foreach (var storage in exportStorages) {
            if (storage.filled) {
                _ready = false;
                return;
            }
        }

        _ready = true;
        onConverterReady?.Invoke();
        StartConvertation();
    }

    private void StartConvertation()
    {
        if (_started) return;

        _workProgress = 0;
        _started = true;

        ConsumeCost();
        onConvertStart?.Invoke();
    }

    private void ConsumeCost()
    {
        foreach (var cost in convertCost) {
            int remains = cost.convertAmount;

            foreach(var storage in importStorages) {
                remains -= storage.SendResource(_converterStorage, cost.type, remains);

                if (remains == 0)
                    break;
            }
        }
    }

    private void SendResult()
    {
        foreach(var result in convertResult) {
            int remains = result.convertAmount;

            foreach (var storage in exportStorages) {
                remains -= _converterStorage.SendResource(storage, result.type, remains);

                if (remains == 0)
                    break;
            }
        }
    }

    private void Convert(float delta)
    {
        _workProgress += workSpeed * delta;

        if (_workProgress >= workAmount) {
            SendResult();
            _started = false;

            CheckReadiness();

            onConvertEnds?.Invoke();
        }
    }

    /// <summary>
    /// ��� � ���������� ������� �� ������ / ������������ ��� ������������
    /// </summary>
    [System.Serializable]
    public struct ConvertInfo
    {
        /// <summary>
        /// ��� �������
        /// </summary>
        public ResourceType type;
        /// <summary>
        /// ���������� ������� �� ������ / ������������ ��� ������������
        /// </summary>
        public int convertAmount;
    }
}
