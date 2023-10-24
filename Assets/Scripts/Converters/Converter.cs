using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Storage))]
public class Converter : MonoBehaviour
{
    [SerializeField] private string converterName = "Без имени";
    [SerializeField] private float workAmount = 1;
    [SerializeField] private float workSpeed = 1;
    [SerializeField] private Storage[] importStorages;
    [SerializeField] private Storage[] exportStorages;
    [SerializeField] private ConvertInfo[] convertCost;
    [SerializeField] private ConvertInfo[] convertResult;
    [SerializeField] private CommonUpgrade commonUpgrade;
    [SerializeField] private ConverterTierUpgrade[] tierUpgrades;

    [SerializeField] private Image progressBar;

    private float _workProgress;

    private float _workSpeedMultiplier = 1;
    private float _capacityMultiplier = 1;
    private int _level = 0;
    private int _tier = 0;

    private Storage _converterStorage;

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
        InitUpgrades();
    }

    private void FixedUpdate()
    {
        if (_started)
            Convert(Time.fixedDeltaTime);

        //UpgradeTier();
    }

    private void ValidateResources()
    {
        if (importStorages == null || importStorages.Length == 0) {
            Debug.Log($"Зоны входа ресурсов преобразователя {converterName} не назначены!");
            enabled = false;
            
            return;
        } if (exportStorages == null || exportStorages.Length == 0) {
            Debug.Log($"Зоны выхода ресурсов преобразователя {converterName} не назначены!");
            enabled = false;

            return;
        } if (convertCost == null) {
            Debug.Log($"Стоимость преобразования в {converterName} не назначена!");
            enabled = false;

            return;
        } if (convertResult == null) {
            Debug.Log($"Результат преобразования в {converterName} не назначена!");
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

    private void Convert(float delta)
    {
        _workProgress += workSpeed * _workSpeedMultiplier * delta;

        if (_workProgress >= workAmount) {
            _workProgress = 0;

            SendResult();
            _started = false;

            CheckReadiness();

            onConvertEnds?.Invoke();
        }

        progressBar.fillAmount = _workProgress / workAmount;
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

    private void InitUpgrades()
    {
        commonUpgrade.onUpgraded += Upgrade;
        tierUpgrades[0].onUpgraded += () => UpgradeTier(tierUpgrades[0]);
    }

    private void Upgrade()
    {
        _workSpeedMultiplier = commonUpgrade.WorkSpeedMultiplier;
        _capacityMultiplier = commonUpgrade.StorageCapacityMultiplier;

        foreach (var storage in importStorages)
            storage.SetCapacityMultiplier(_capacityMultiplier);
        foreach (var storage in exportStorages)
            storage.SetCapacityMultiplier(_capacityMultiplier);
    }

    private void UpgradeTier(ConverterTierUpgrade tier)
    {
        convertCost = tier.tierConvertCost;
        convertResult = tier.tierConvertResult;

        Upgrade nextTier = tier.Next;
    }
}

/// <summary>
/// Тип и количество ресурса на выходе / необходимого для производства
/// </summary>
[System.Serializable]
public struct ConvertInfo
{
    /// <summary>
    /// Тип ресурса
    /// </summary>
    public ResourceType type;
    /// <summary>
    /// Количество ресурса на выходе / необходимого для производства
    /// </summary>
    public int convertAmount;
}
