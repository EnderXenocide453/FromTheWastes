using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Storage))]
public class Converter : MonoBehaviour
{
    #region Поля
    [SerializeField] private string converterName = "Без имени";
    [SerializeField] private float workAmount = 1;
    [SerializeField] private float workSpeed = 1;

    [SerializeField] private ConvertInfo[] convertCost;
    [SerializeField] private ConvertInfo[] convertResult;

    [SerializeField] private EmployeeCarrier employee;

    [SerializeField] private Image progressBar;
    [SerializeField] private UpgradeArea upgradeArea;

    public Storage[] importStorages;
    public Storage[] exportStorages;
    public ConverterUpgrader upgrader;
    
    private float _workProgress;

    private float _workSpeedMultiplier = 1;
    private float _capacityMultiplier = 1;

    private int _level;
    private int _tier;

    private bool _started;

    private Storage _converterStorage;

    private Dictionary<ResourceType, int> _readyResources;
    #endregion

    #region События
    public delegate void ConverterHandler();
    public event ConverterHandler onConvertEnds;
    public event ConverterHandler onConvertStart;
    public event ConverterHandler onConverterReady;
    public event ConverterHandler onConvertChange;
    #endregion

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

        foreach (var storage in exportStorages)
            storage.onCountChanged += (object obj) => {
                if (!_started)
                    CheckReadiness();
            };
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
        foreach (var cost in convertCost)
            if (_readyResources[cost.type] < cost.amount)
                return;

        foreach (var storage in exportStorages)
            if (storage.filled)
                return;

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
            int remains = cost.amount;

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
            OnConvertEnds();
        }

        progressBar.fillAmount = _workProgress / workAmount;
    }

    private void OnConvertEnds()
    {
        SendResult();
        _started = false;

        CheckReadiness();

        onConvertEnds?.Invoke();
    }

    private void SendResult()
    {
        foreach(var result in CalculateResult()) {
            int remains = result.amount;

            foreach (var storage in exportStorages) {
                remains -= _converterStorage.SendResource(storage, result.type, remains);
                
                if (remains == 0)
                    break;
            }
        }
    }

    protected virtual ConvertInfo[] CalculateResult()
    {
        return convertResult;
    }

    private void InitUpgrades()
    {
        upgrader.Init();
        upgradeArea.upgrader = upgrader;

        upgrader.onConverterUpgrade += Upgrade;
        upgrader.onTierUpgrade += () => UpgradeTier(upgrader.CurrentTier);
        upgrader.onEmployeeUpgrade += UpgradeEmployee;
    }

    private void Upgrade()
    {
        _workSpeedMultiplier = upgrader.ConverterUpgrade.WorkSpeedMultiplier;
        _capacityMultiplier = upgrader.ConverterUpgrade.StorageCapacityMultiplier;
        
        foreach (var storage in importStorages)
            storage.SetCapacityMultiplier(_capacityMultiplier);
        foreach (var storage in exportStorages)
            storage.SetCapacityMultiplier(_capacityMultiplier);

        _level++;
    }

    private void UpgradeTier(ConverterTierUpgrade tier)
    {
        convertCost = tier.tierConvertCost;
        convertResult = tier.tierConvertResult;

        onConvertChange?.Invoke();

        _tier++;
    }

    private void UpgradeEmployee()
    {
        employee.SetCapacityModifier(upgrader.EmployeeUpgrade.StorageCapacityMultiplier);
        employee.SetWorkSpeedModifier(upgrader.EmployeeUpgrade.WorkSpeedMultiplier);
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
    public int amount;

    public ConvertInfo(ResourceType type, int amount)
    {
        this.type = type;
        this.amount = amount;
    }
}
