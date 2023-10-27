using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLoader : MonoBehaviour
{
    private Dictionary<int, SaveInfo> _saveInfo;
    private int _count = 0;
    
    public delegate void LoaderHandler();
    public event LoaderHandler onSave;

    void Awake()
    {
        SaveItemCollector.Loader = this;

        _saveInfo = new Dictionary<int, SaveInfo>();
    }

    public void LoadGame()
    {

    }

    public void SaveGame()
    {

    }

    public void AddSaveItem(SaveItem item)
    {
        _saveInfo.Add(_count, item.SaveInfo);
        //ѕрисваивание нового id и его инкремент
        item.saveID = _count++;
    }
}

public static class SaveItemCollector
{
    public static GameLoader Loader { 
        set
        {
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