using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Класс объекта, следующего за другим объектом
/// </summary>
public class ObjectFollow : MonoBehaviour
{
    /// <summary>
    /// Объект следования
    /// </summary>
    [SerializeField] private Transform target;
    /// <summary>
    /// Скорость движения
    /// </summary>
    [SerializeField] private float speed = 0.5f;

    private void FixedUpdate()
    {
        Move();
    }

    /// <summary>
    /// Переместиться к объекту следования
    /// </summary>
    private void Move()
    {
        transform.position = Vector3.Lerp(transform.position, target.position, speed);
    }
}
