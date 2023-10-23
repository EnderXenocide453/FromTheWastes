//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class Converter : MonoBehaviour
//{
//    [SerializeField] private string converterName = "Без имени";
//    [SerializeField] private Storage[] importStorages;
//    [SerializeField] private Storage[] exportStorages;
//    [SerializeField] private ConvertInfo[] convertCost;
//    [SerializeField] private ConvertInfo[] convertResult;
//    [SerializeField] private float workAmount = 1;
//    [SerializeField] private float workSpeed = 1;

//    private Storage _converterStorage;
//    private float _workProgress;

//    private Dictionary<ResourceType, int> _readyResources;

//    private bool _ready;
//    private bool _started;

//    public delegate void ConverterHandler();
//    public event ConverterHandler onConvertEnds;
//    public event ConverterHandler onConvertStart;
//    public event ConverterHandler onConverterReady;

//    private void Start()
//    {
//        ValidateResources();
//        InitStorage();
//    }

//    private void FixedUpdate()
//    {
//        if (_started)
//            Convert(Time.fixedDeltaTime);
//    }

//    private void ValidateResources()
//    {
//        if (importStorages == null || importStorages.Length == 0) {
//            Debug.Log($"Зоны входа ресурсов преобразователя {converterName} не назначены!");
//            enabled = false;
            
//            return;
//        } if (exportStorages == null || exportStorages.Length == 0) {
//            Debug.Log($"Зоны выхода ресурсов преобразователя {converterName} не назначены!");
//            enabled = false;

//            return;
//        } if (convertCost == null) {
//            Debug.Log($"Стоимость преобразования в {converterName} не назначена!");
//            enabled = false;

//            return;
//        } if (convertResult == null) {
//            Debug.Log($"Результат преобразования в {converterName} не назначена!");
//            enabled = false;

//            return;
//        }
//    }

//    private void InitStorage()
//    {
//        _readyResources = new Dictionary<ResourceType, int>();

//        foreach(var res in convertCost)
//            _readyResources.TryAdd(res.type, 0);
//    }

//    private void CheckImportResource(int id)
//    {
//        int count = convertationImport[id].connectedStorage.GetCount();

//        if (count >= convertationImport[id].convertAmount) {
//            _resourcesReady[id] = true;
//            CheckReadiness();
//        }
//        else
//            _resourcesReady[id] = false;
//    }

//    private void CheckReadiness()
//    {
//        foreach (bool ready in _resourcesReady)
//            if (!ready) return;

//        onConverterReady?.Invoke();
//    }

//    private void StartConvertation()
//    {
//        _workProgress = 0;
//        _started = true;

//        ConsumeCost();
//        onConvertStart?.Invoke();
//    }

//    private void ConsumeCost()
//    {
//        foreach(var storage in convertationImport) {
//            int resourceRemains = storage.convertAmount;

//            foreach(var type in storage.ResourceTypes) {
//                int sendCount = storage.connectedStorage.GetResourceCount(type);
//                sendCount = sendCount > resourceRemains ? resourceRemains : sendCount;

//                storage.connectedStorage.SendResource(_converterStorage, type, sendCount);

//                resourceRemains -= sendCount;
                
//                if (resourceRemains == 0)
//                    break;
//            }
//        }
//    }

//    private void SendResult()
//    {
//        foreach(var storage)
//    }

//    private void Convert(float delta)
//    {
//        _workProgress += workSpeed * delta;

//        if (_workProgress >= workAmount) {
//            SendResult();
//            _started = false;

//            CheckReadiness();

//            onConvertEnds?.Invoke();
//        }
//    }

//    /// <summary>
//    /// Хранилище и количество ресурса на выходе / необходимого для производства
//    /// </summary>
//    [System.Serializable]
//    public struct ConvertInfo
//    {
//        /// <summary>
//        /// Тип ресурса
//        /// </summary>
//        public ResourceType type;
//        /// <summary>
//        /// Количество ресурса на выходе / необходимого для производства
//        /// </summary>
//        public int convertAmount;
//    }
//}
