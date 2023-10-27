using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLoader : MonoBehaviour
{
    private Dictionary<int, SaveItem> _saveItems;
    private Dictionary<int, SaveInfo> _saveInfo;
    private int _count = 0;
    
    public delegate void LoaderHandler();
    public event LoaderHandler onSave;
    public event LoaderHandler onLoad;

    void Awake()
    {
        SaveItemCollector.Loader = this;
    }

    [ContextMenu("LoadGame")]
    public void LoadGame()
    {
        foreach (var item in _saveItems)
            item.Value.Load(_saveInfo[item.Key]);
    }

    [ContextMenu("SaveGame")]
    public void SaveGame()
    {
        _saveInfo = new Dictionary<int, SaveInfo>();

        foreach (var item in _saveItems) {
            _saveInfo.Add(item.Key, item.Value.SaveInfo);
        }
    }

    public void AddSaveItem(SaveItem item)
    {
        if (_saveItems == null)
            _saveItems = new Dictionary<int, SaveItem>();

        _saveItems.Add(_count, item);
        //ѕрисваивание нового id и его инкремент
        item.saveID = _count++;
    }
}

public static class SaveItemCollector
{
    public static GameLoader Loader { 
        set
        {
            Debug.Log("a");
            if (_loader) return;

            _loader = value;
        } 
    }

    private static GameLoader _loader;

    public static void AddSaveItem(SaveItem item)
    {
        _loader.AddSaveItem(item);
    }
}