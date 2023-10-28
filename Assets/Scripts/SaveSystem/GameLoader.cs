using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class GameLoader : MonoBehaviour
{
    private Dictionary<int, SaveItem> _saveItems;
    private GlobalSaveObject _saveInfo;
    private int _count = 0;
    
    public delegate void LoaderHandler();
    public event LoaderHandler onSave;
    public event LoaderHandler onLoad;

    void Awake()
    {
        SaveItemCollector.Loader = this;
        _saveInfo = new GlobalSaveObject();
    }

    [ContextMenu("LoadGame")]
    public void LoadGame()
    {
        if (File.Exists(Application.persistentDataPath + "/save.dat")) {
            BinaryFormatter bf = new BinaryFormatter();
            SurrogateSelector ss = new SurrogateSelector();
            var streamingContext = new StreamingContext(StreamingContextStates.All);

            ss.AddSurrogate(typeof(Vector3), streamingContext, new Vector3SerializationSurrogate());
            ss.AddSurrogate(typeof(Quaternion), streamingContext, new QuaternionSerializationSurrogate());

            bf.SurrogateSelector = ss;

            FileStream file = File.Open(Application.persistentDataPath + "/save.dat", FileMode.Open);

            _saveInfo = (GlobalSaveObject)bf.Deserialize(file);

            foreach (var item in _saveItems)
                item.Value.Load(_saveInfo.saveInfo[item.Key]);

            GlobalValues.Cash = _saveInfo.cash;
        }
    }

    [ContextMenu("SaveGame")]
    public void SaveGame()
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

        FileStream file = File.Create(Application.persistentDataPath + "/save.dat");

        bf.Serialize(file, _saveInfo);
        file.Close();
    }

    public void AddSaveItem(SaveItem item)
    {
        if (_saveItems == null)
            _saveItems = new Dictionary<int, SaveItem>();

        _saveItems.Add(_count, item);
        //ѕрисваивание нового id и его инкремент
        item.saveID = _count++;
    }

    [System.Serializable]
    private struct GlobalSaveObject
    {
        public Dictionary<int, SaveInfo> saveInfo;
        public int cash;
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