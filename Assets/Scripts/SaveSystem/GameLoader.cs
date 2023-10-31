using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

/// <summary>
/// Контроллер сохранения и загрузки игры
/// </summary>
public class GameLoader : MonoBehaviour
{
    [SerializeField] private float autosaveDelay;
    [SerializeField] private bool loadOnStart = true;
    [SerializeField] private bool autosave = true;

    private Dictionary<int, SaveItem> _saveItems;
    private GlobalSaveObject _saveInfo;
    private int _count = 0;
    
    public delegate void LoaderHandler();
    public event LoaderHandler onSave;
    public event LoaderHandler onLoad;

    void Awake()
    {
        Time.timeScale = 1;

        SaveItemCollector.Loader = this;
        _saveInfo = new GlobalSaveObject();

        if (loadOnStart)
            LoadGame(GlobalValues.newGame ? GlobalValues.newSaveName : GlobalValues.saveName);

        if (autosave)
            StartCoroutine(Autosave());
    }

    private void OnApplicationQuit()
    {
        if (autosave)
            SaveGame();
    }

    /// <summary>
    /// Загрузить игру
    /// </summary>
    /// <param name="name">Имя файла сохранения</param>
    public void LoadGame(string name = GlobalValues.saveName)
    {
        if (File.Exists(Application.persistentDataPath + $"/{name}.dat")) {
            BinaryFormatter bf = new BinaryFormatter();
            SurrogateSelector ss = new SurrogateSelector();
            var streamingContext = new StreamingContext(StreamingContextStates.All);

            ss.AddSurrogate(typeof(Vector3), streamingContext, new Vector3SerializationSurrogate());
            ss.AddSurrogate(typeof(Quaternion), streamingContext, new QuaternionSerializationSurrogate());

            bf.SurrogateSelector = ss;

            FileStream file = File.Open(Application.persistentDataPath + $"/{name}.dat", FileMode.Open);

            _saveInfo = (GlobalSaveObject)bf.Deserialize(file);

            foreach (var item in _saveInfo.saveInfo) {
                Debug.Log($"Загрузка {item.Value.prefabPath}");
                SaveItem newItem = Instantiate(Resources.Load<GameObject>(item.Value.prefabPath)).GetComponent<SaveItem>();
                newItem.SetLoadInfo(_saveInfo.saveInfo[item.Key]);
            }

            GlobalValues.Cash = _saveInfo.cash;
        }
    }

    /// <summary>
    /// Сохранить игру
    /// </summary>
    /// <param name="name">имя сохранения</param>
    public void SaveGame(string name = GlobalValues.saveName)
    {
        _saveInfo.saveInfo = new Dictionary<int, SaveInfo>();

        foreach (var item in _saveItems) {
            _saveInfo.saveInfo.Add(item.Key, item.Value.SaveInfo);
        }

        _saveInfo.cash = GlobalValues.Cash;

        BinaryFormatter bf = new BinaryFormatter();
        SurrogateSelector ss = new SurrogateSelector();
        var streamingContext = new StreamingContext(StreamingContextStates.All);

        ss.AddSurrogate(typeof(Vector3), streamingContext, new Vector3SerializationSurrogate());
        ss.AddSurrogate(typeof(Quaternion), streamingContext, new QuaternionSerializationSurrogate());

        bf.SurrogateSelector = ss;

        FileStream file = File.Create(Application.persistentDataPath + $"/{name}.dat");

        bf.Serialize(file, _saveInfo);
        file.Close();
    }

    /// <summary>
    /// Добавить объект сохранения
    /// </summary>
    /// <param name="item"></param>
    public void AddSaveItem(SaveItem item)
    {
        if (_saveItems == null)
            _saveItems = new Dictionary<int, SaveItem>();

        _saveItems.Add(_count, item);
        //Присваивание нового id и его инкремент
        item.saveID = _count++;
    }

    #if UNITY_EDITOR
    /// <summary>
    /// Сохранение состояния игры в качестве начального
    /// </summary>
    [ContextMenu("Rewrite new game")]
    public void SaveNewGame()
    {
        SaveGame(GlobalValues.newSaveName);
    }
    #endif

    /// <summary>
    /// Автоматическое сохранение
    /// </summary>
    /// <returns></returns>
    private IEnumerator Autosave()
    {
        while (true) {
            yield return new WaitForSeconds(autosaveDelay);

            SaveGame();
        }
    }

    /// <summary>
    /// Структура объекта сохранения
    /// </summary>
    [System.Serializable]
    private struct GlobalSaveObject
    {
        public Dictionary<int, SaveInfo> saveInfo;
        public int cash;
    }
}

/// <summary>
/// Статический класс для обращения к загрузчику из любого участка кода
/// </summary>
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

/// <summary>
/// Класс для сериализации векторов
/// </summary>
public class Vector3SerializationSurrogate : ISerializationSurrogate
{
    public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
    {
        Vector3 v3 = (Vector3)obj;
        info.AddValue("x", v3.x);
        info.AddValue("y", v3.y);
        info.AddValue("z", v3.z);
    }

    public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
    {
        Vector3 v3 = (Vector3)obj;
        v3.x = info.GetSingle("x");
        v3.y = info.GetSingle("y");
        v3.z = info.GetSingle("z");
        obj = v3;
        return obj;
    }
}

/// <summary>
/// Класс для сериализации кватернионов
/// </summary>
public class QuaternionSerializationSurrogate : ISerializationSurrogate
{
    public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
    {
        Quaternion q = (Quaternion)obj;
        info.AddValue("x", q.x);
        info.AddValue("y", q.y);
        info.AddValue("z", q.z);
        info.AddValue("w", q.w);
    }

    public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
    {
        Quaternion q = (Quaternion)obj;
        q.x = info.GetSingle("x");
        q.y = info.GetSingle("y");
        q.z = info.GetSingle("z");
        q.w = info.GetSingle("w");
        obj = q;
        return obj;
    }
}