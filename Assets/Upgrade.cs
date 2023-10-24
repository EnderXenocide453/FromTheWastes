using System;
using UnityEngine;

[System.Serializable]
public abstract class Upgrade
{
    [SerializeField] protected int baseCost;
    [SerializeField] protected int costModifier;
    [SerializeField] protected string description = "NaN";
    [SerializeField] protected string name = "NaN";
    [SerializeField] protected Upgrade nextUpgrade;
    public delegate void UpgradeHandler();

    public event UpgradeHandler onUpgraded;

    public abstract int Cost { get; }
    public abstract string Description { get; }
    public abstract string Name { get; }
    public abstract Upgrade Next { get; }
    public void DoUpgrade()
    {
        Init();
        onUpgraded?.Invoke();
    }

    public abstract void Init();
}

[Serializable]
public class CommonUpgrade : Upgrade
{
    [SerializeField] protected int currentLevel;
    [SerializeField] protected int maxLevel;
    [SerializeField] protected float storageCapacityMultiplier;
    [SerializeField] protected float workSpeedMultiplier;

    public event UpgradeHandler onMaxLevel;

    public override int Cost { get => (currentLevel * costModifier + 1) * baseCost; }

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
        get => nextUpgrade;
    }
    public float StorageCapacityMultiplier { get => currentLevel * storageCapacityMultiplier; }
    public float WorkSpeedMultiplier { get => currentLevel * workSpeedMultiplier; }

    public CommonUpgrade(CommonUpgrade previous)
    {
        currentLevel = previous.currentLevel + 1;
        maxLevel = previous.maxLevel;

        name = previous.name;
        description = previous.description;

        if (currentLevel == maxLevel)
            onMaxLevel?.Invoke();
    }

    public override void Init()
    {
        if (currentLevel <= maxLevel) {
            nextUpgrade = new CommonUpgrade(this);
            return;
        }

        nextUpgrade = null;
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
    public override Upgrade Next { get => nextUpgrade; }
    public override void Init()
    {
        if (tierParts == null)
            return;

        foreach (var part in tierParts)
            part.gameObject.SetActive(true);
    }
}
