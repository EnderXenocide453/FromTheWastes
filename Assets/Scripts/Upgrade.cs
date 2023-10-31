using System;
using System.Collections.Generic;
using UnityEngine;

#region Улучшения
/// <summary>
/// Базовый класс улучшения
/// </summary>
[System.Serializable]
public abstract class Upgrade
{
    [SerializeField] protected string name = "NaN";
    [SerializeField, TextArea] protected string description = "NaN";
    [SerializeField] protected int baseCost;
    [SerializeField] protected float costModifier;
    
    protected Upgrade _nextUpgrade;
    
    public bool enabled = true;
    
    public delegate void UpgradeHandler();
    public event UpgradeHandler onUpgraded;

    public abstract int Cost { get; }
    public abstract string Description { get; }
    public abstract string Name { get; }
    public abstract Upgrade Next { get; }
    
    /// <summary>
    /// Произвести улучшение
    /// </summary>
    public void DoUpgrade()
    {
        PreUpgrade();
        OnUpgraded();
        PostUpgrade();
    }

    /// <summary>
    /// Установить улучшение как следующее за этим
    /// </summary>
    /// <param name="next">Следующее улучшение</param>
    public void SetNext(Upgrade next)
    {
        _nextUpgrade = next;
    }

    /// <summary>
    /// Удаление слушателей события
    /// </summary>
    public void RemoveListeners()
    {
        onUpgraded = null;
    }

    /// <summary>
    /// Действия перед улучшением
    /// </summary>
    public abstract void PreUpgrade();
    /// <summary>
    /// Действия после улучшения
    /// </summary>
    public abstract void PostUpgrade();

    /// <summary>
    /// Вызывается при произведении улучшения
    /// </summary>
    protected virtual void OnUpgraded()
    {
        onUpgraded?.Invoke();
    }
}

/// <summary>
/// Класс поуровневого улучшения сущности
/// </summary>
[Serializable]
public class CommonUpgrade : Upgrade
{
    [SerializeField] protected int maxLevel = 10;
    [SerializeField] protected float storageCapacityMultiplier;
    [SerializeField] protected float workSpeedMultiplier;
    public int currentLevel { get; private set; } = 0;

    public event UpgradeHandler onMaxLevel;

    public override int Cost { get => Mathf.RoundToInt((currentLevel * costModifier + 1) * baseCost); }

    public override string Description
    {
        get => $"{description}\nРазмер хранилища + {storageCapacityMultiplier * 100}%\nСкорость работы + {workSpeedMultiplier * 100}%";
    }

    public override string Name
    {
        get => $"{name} уровень {currentLevel + 1}";
    }
    public override Upgrade Next
    {
        get => _nextUpgrade;
    }
    public float StorageCapacityMultiplier { get => 1 + storageCapacityMultiplier * currentLevel; }
    public float WorkSpeedMultiplier { get => 1 + workSpeedMultiplier * currentLevel; }

    public override void PreUpgrade()
    {
        if (currentLevel < maxLevel) {
            currentLevel++;
            _nextUpgrade = this;
            return;
        }
    }

    public override void PostUpgrade()
    {
        if (currentLevel < maxLevel)
            return;

        RemoveListeners();
        enabled = false;
        _nextUpgrade = null;
        onMaxLevel?.Invoke();
    }

    /// <summary>
    /// Улучшение до заданного уровня
    /// </summary>
    /// <param name="level"></param>
    public void UpgradeTo(int level)
    {
        currentLevel = Mathf.Clamp(level, 0, maxLevel);
        _nextUpgrade = this;

        OnUpgraded();

        PostUpgrade();
    }
}

/// <summary>
/// Класс улучшения по кругам/тирам. Активирует скрытые объекты
/// </summary>
[Serializable]
public class TierUpgrade : Upgrade
{
    /// <summary>
    /// Объекты, открываемые при улучшении
    /// </summary>
    public Transform[] tierParts;
    public override int Cost { get => baseCost; }
    public override string Description { get => description; }
    public override string Name { get => name; }
    public override Upgrade Next { get => _nextUpgrade; }

    public override void PostUpgrade() { }

    public override void PreUpgrade()
    {
        if (tierParts == null)
            return;

        foreach (var part in tierParts)
            part.gameObject.SetActive(true);
    }
}

/// <summary>
/// Подкласс улучшения по тирам. Меняет производственные данные преобразователя
/// </summary>
[Serializable]
public class ConverterTierUpgrade : TierUpgrade
{
    public ConvertInfo[] tierConvertCost;
    public ConvertInfo[] tierConvertResult;
}
#endregion

#region Контроллеры улучшений
/// <summary>
/// Базовый класс контроллера улучшений. Хранит данные об улучшении
/// </summary>
[System.Serializable]
public abstract class Upgrader
{
    /// <summary>
    /// Доступные в данный момент улучшения
    /// </summary>
    public abstract Upgrade[] CurrentUpgrades { get; }
    /// <summary>
    /// Состояние контроллера для сохранения
    /// </summary>
    public abstract int[] SaveInfo { get; }

    public delegate void UpgraderHandler();

    /// <summary>
    /// Инициализация улучшений
    /// </summary>
    public abstract void Init();
    /// <summary>
    /// Улучшить до указанного состояния
    /// </summary>
    /// <param name="saveInfo"></param>
    public abstract void UpgradeTo(int[] saveInfo);
}

/// <summary>
/// Контроллер улучшений преобразователя
/// </summary>
[System.Serializable]
public class ConverterUpgrader : Upgrader
{
    [SerializeField] private CommonUpgrade converterUpgrade;
    [SerializeField] private ConverterTierUpgrade[] tierUpgrades;
    [SerializeField] private TierUpgrade hireEmployee;
    [SerializeField] private CommonUpgrade employeeUpgrade;

    private int _nextTier;
    private bool _hired;

    public event UpgraderHandler onConverterUpgrade;
    public event UpgraderHandler onTierUpgrade;
    public event UpgraderHandler onHireEmployee;
    public event UpgraderHandler onEmployeeUpgrade;

    public ConverterTierUpgrade CurrentTier { get => (_nextTier >= tierUpgrades.Length || _nextTier < 0) ? null : tierUpgrades[_nextTier - 1]; }
    public ConverterTierUpgrade NextTier { get => _nextTier >= tierUpgrades.Length ? null : tierUpgrades[_nextTier]; }
    public CommonUpgrade ConverterUpgrade { get => converterUpgrade.enabled ? converterUpgrade : null; }
    public CommonUpgrade EmployeeUpgrade { get => employeeUpgrade.enabled ? employeeUpgrade : null; }

    public override Upgrade[] CurrentUpgrades
    {
        get
        {
            return new Upgrade[]
            {
                ConverterUpgrade,
                NextTier,
                _hired ? EmployeeUpgrade : hireEmployee
            };
        }
    }

    public override int[] SaveInfo {
        get => new int[]
        {
            converterUpgrade.currentLevel,
            _nextTier,
            _hired ? employeeUpgrade.currentLevel : -1
        };
    }

    public override void Init()
    {
        converterUpgrade.onUpgraded += () => onConverterUpgrade?.Invoke();

        employeeUpgrade.onUpgraded += () => onEmployeeUpgrade?.Invoke();

        hireEmployee.onUpgraded += HireEmployee;

        if (tierUpgrades.Length == 0)
            return;

        ConverterTierUpgrade tier = tierUpgrades[tierUpgrades.Length - 1];
        tier.onUpgraded += () => UpgradeTier();

        _nextTier = 0;

        for (int i = 0; i < tierUpgrades.Length - 1; i++) {
            tier = tierUpgrades[i];
            tier.onUpgraded += () => UpgradeTier();
            tier.SetNext(tierUpgrades[i + 1]);
        }
    }

    public override void UpgradeTo(int[] saveInfo)
    {
        Debug.Log(saveInfo[1]);

        Init();

        converterUpgrade.UpgradeTo(saveInfo[0]);

        int curTier = saveInfo[1] - 1;

        for (int i = 0; i < curTier; i++)
            tierUpgrades[i].DoUpgrade();

        //Если работник не нанят выходим из метода
        if (saveInfo[2] < 0)
            return;

        hireEmployee.DoUpgrade();
        employeeUpgrade.UpgradeTo(saveInfo[2]);
    }

    /// <summary>
    /// Улучшить круг
    /// </summary>
    private void UpgradeTier()
    {
        _nextTier++;
        onTierUpgrade?.Invoke();
        
    }

    /// <summary>
    /// Нанять работника
    /// </summary>
    private void HireEmployee()
    {
        hireEmployee.SetNext(employeeUpgrade);

        onHireEmployee?.Invoke();
        _hired = true;
    }
}

/// <summary>
/// Класс контроллера улучшений персонажа игрока
/// </summary>
[System.Serializable]
public class PlayerUpgrader : Upgrader
{
    [SerializeField] private CommonUpgrade upgrade;

    public event UpgraderHandler onUpgraded;

    public CommonUpgrade PlayerUpgrade { get => upgrade.enabled ? upgrade : null; }

    public override Upgrade[] CurrentUpgrades { get => new Upgrade[] { PlayerUpgrade }; }

    public override int[] SaveInfo { get => new int[] { upgrade.currentLevel }; }

    public override void Init()
    {
        upgrade.onUpgraded += () => onUpgraded?.Invoke();
    }

    public override void UpgradeTo(int[] saveInfo)
    {
        Init();

        upgrade.UpgradeTo(saveInfo[0]);
    }
}
#endregion