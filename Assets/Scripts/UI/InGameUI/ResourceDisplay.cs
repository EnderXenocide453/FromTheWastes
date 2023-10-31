using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Класс отображения ресурсов
/// </summary>
public class ResourceDisplay : MonoBehaviour
{
    [SerializeField] private GameObject rowPrefab;

    private Dictionary<ResourceType, int> _resources;
    private Dictionary<ResourceType, (Transform row, Image icon, TMP_Text counter)> _rows;

    private void Awake()
    {
        Init();
    }

    /// <summary>
    /// Отобразить количество ресурса
    /// </summary>
    /// <param name="type">Тип ресурса</param>
    /// <param name="count">Количество ресурса</param>
    public void SetResource(ResourceType type, int count)
    {
        if (_resources == null)
            return;
        
        if (!_resources.ContainsKey(type)) {
            _resources.TryAdd(type, 0);
            _rows.TryAdd(type, AddRow(type));
        }

        _resources[type] = count;
        UpdateRow(type);
    }

    /// <summary>
    /// Инициализация отображения ресурсов
    /// </summary>
    private void Init()
    {
        _resources = new Dictionary<ResourceType, int>();
        _rows = new Dictionary<ResourceType, (Transform row, Image icon, TMP_Text counter)>();
    }

    /// <summary>
    /// Добавить строку состояния ресурса
    /// </summary>
    /// <param name="type">Тип ресурса</param>
    /// <returns>UI строки</returns>
    private (Transform, Image, TMP_Text) AddRow(ResourceType type)
    {
        Transform row = Instantiate(rowPrefab, transform).transform;

        Image icon = row.GetComponentInChildren<Image>();
        icon.sprite = Resources.Load<Sprite>(ResourcesCollection.GetResource(type).iconPath);

        return (row, icon, row.GetComponentInChildren<TMP_Text>());
    }

    /// <summary>
    /// Обновить UI ресурса
    /// </summary>
    /// <param name="type"></param>
    private void UpdateRow(ResourceType type)
    {
        if (!_resources.ContainsKey(type))
            return;

        _rows[type].row.gameObject.SetActive(_resources[type] > 0);
        _rows[type].counter.text = _resources[type].ToString();
    }
}