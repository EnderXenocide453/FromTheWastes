using System;
using System.Collections.Generic;
using UnityEngine;

#region Улучшения
[System.Serializable]
public abstract class Upgrade
{
    [SerializeField] protected string name = "NaN";
    [SerializeField, TextArea] protected string description = "NaN";
    [SerializeField] protected int baseCost;
    [SerializeField] protected float costModifier;
    
    protected Upgrade _nextUpgrade;
    
    public delegate void UpgradeHandler();

    public event UpgradeHandler onUpgraded;

    public abstract int Cost { get; }
    public abstract string Description { get; }
    public abstract string Name { get; }
    public abstract Upgrade Next { get; }
    
    public void DoUpgrade()
    {
        EnableUpgrade();
        onUpgraded?.Invoke();
    }

    public void SetNext(Upgrade next)
    {
        _nextUpgrade = next;
    }

    public abstract void EnableUpgrade();
}

[Serializable]
public class CommonUpgrade : Upgrade
{
    [SerializeField] protected int currentLevel = 1;
    [SerializeField] protected int maxLevel = 10;
    [SerializeField] protected float storageCapacityMultiplier;
    [SerializeField] protected float workSpeedMultiplier;

    public event UpgradeHandler onMaxLevel;

    public override int Cost { get => Mathf.RoundToInt((currentLevel * costModifier + 1) * baseCost); }

    public override string Description
    {
        get => $"{description}\nРазмер хранилища + {storageCapacityMultiplier * 100}%\nСкорость работы + {workSpeedMultiplier * 100}%";
    }

    public override string Name
    {
        get => $"{name} уровень {currentLevel}";
    }
    public override Upgrade Next
    {
        get => _nextUpgrade;
    }
    public float StorageCapacityMultiplier { get => 1 + storageCapacityMultiplier * currentLevel; }
    public float WorkSpeedMultiplier { get => 1 + workSpeedMultiplier * currentLevel; }

    public CommonUpgrade(CommonUpgrade previous)
    {
        currentLevel = previous.currentLevel + 1;
        maxLevel = previous.maxLevel;

        name = previous.name;
        description = previous.description;

        storageCapacityMultiplier = previous.storageCapacityMultiplier;
        workSpeedMultiplier = previous.workSpeedMultiplier;

        if (currentLevel == maxLevel)
            onMaxLevel?.Invoke();
    }

    public override void EnableUpgrade()
    {
        if (currentLevel <= maxLevel) {
            _nextUpgrade = new CommonUpgrade(this);
            return;
        }

        _nextUpgrade = null;
    }
}

[Serializable]
public class ConverterTierUpgrade : Upgrade
{
    public ConvertInfo[] tierConvertCost;
    public ConvertInfo[] tierConvertResult;
    public Transform[] tierParts;
    public override int Cost { get => baseCost; }
    public override string Description { get => description; }
    public override string Name { get => name; }
    public override Upgrade Next { get => _nextUpgrade; }
    public override void EnableUpgrade()
    {
        if (tierParts == null)
            return;

        foreach (var part in tierParts)
            part.gameObject.SetActive(true);
    }
}
#endregion

#region Контроллеры улучшений
[System.Serializable]
public abstract class Upgrader
{
    public abstract Upgrade[] CurrentUpgrades { get; }

    public delegate void UpgraderHandler();

    /// <summary>
    /// Инициализация улучшений
    /// </summary>
    protected abstract void Init();
}

[System.Serializable]
public class ConverterUpgrader : Upgrader
{
    [SerializeField] private CommonUpgrade commonUpgrade;
    [SerializeField] private ConverterTierUpgrade[] tierUpgrades;

    private int _curTier;

    public event UpgraderHandler onCommonUpgrade;
    public event UpgraderHandler onTierUpgrade;

    public ConverterTierUpgrade CurrentTier { get => tierUpgrades[_curTier]; }
    public CommonUpgrade CommonUpgrade { get => commonUpgrade; }

    public override Upgrade[] CurrentUpgrades
    {
        get
        {
            return new Upgrade[]
            {
                CommonUpgrade,
                CurrentTier
            };
        }
    }

    protected override void Init()
    {
        commonUpgrade.onUpgraded += Upgrade;

        ConverterTierUpgrade tier = tierUpgrades[tierUpgrades.Length - 1];
        tier.onUpgraded += () => UpgradeTier();

        for (int i = 0; i < tierUpgrades.Length - 1; i++) {
            tier = tierUpgrades[i];
            tier.onUpgraded += () => UpgradeTier();
            tier.SetNext(tierUpgrades[i + 1]);
        }
    }

    private void Upgrade()
    {
        onCommonUpgrade?.Invoke();
        
        commonUpgrade = (CommonUpgrade)commonUpgrade.Next;
        if (commonUpgrade != null)
            commonUpgrade.onUpgraded += Upgrade;
    }

    private void UpgradeTier()
    {
        onTierUpgrade?.Invoke();

        _curTier++;
    }
}
#endregion