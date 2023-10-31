using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ����� �������, ���������� �� ������ ��������
/// </summary>
public class ObjectFollow : MonoBehaviour
{
    /// <summary>
    /// ������ ����������
    /// </summary>
    [SerializeField] private Transform target;
    /// <summary>
    /// �������� ��������
    /// </summary>
    [SerializeField] private float speed = 0.5f;

    private void FixedUpdate()
    {
        Move();
    }

    /// <summary>
    /// ������������� � ������� ����������
    /// </summary>
    private void Move()
    {
        transform.position = Vector3.Lerp(transform.position, target.position, speed);
    }
}
