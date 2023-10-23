using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Storage))]
public class Converter : MonoBehaviour
{
    [SerializeField] private Storage[] importStorages;
    [SerializeField] private Storage[] exportStorages;

    private Storage _converterStorage;
}
