using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Класс объекта сохранения
/// </summary>
public abstract class SaveItem : MonoBehaviour
{
    public string path;
    public int saveID;

    private SaveInfo _loadInfo;

    public abstract SaveInfo SaveInfo { get; }

    protected virtual void Start()
    {
        //После появления добвляется в список объектов сохранения загрузчика
        SaveItemCollector.AddSaveItem(this);

        //Если есть что загружать, загружаем
        if (_loadInfo != null)
            Load(_loadInfo);
    }

    /// <summary>
    /// Установить информацию загрузки
    /// </summary>
    /// <param name="info">Состояние объекта для загрузки</param>
    public void SetLoadInfo(SaveInfo info)
    {
        _loadInfo = info;
    }

    /// <summary>
    /// Загрузить состояние
    /// </summary>
    /// <param name="info">Состояние</param>
    protected abstract void Load(SaveInfo info);
}

/// <summary>
/// Состояние объекта сохранения
/// </summary>
[System.Serializable]
public class SaveInfo
{
    public string prefabPath;
    [SerializeField] public Vector3 position;
    public Quaternion rotation;

    public int[] upgraderInfo;
    public Dictionary<ResourceType, int>[] storagesInfo;
}
