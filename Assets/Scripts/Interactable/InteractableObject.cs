using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ����������� ����� ������������� ��������
/// </summary>
public abstract class InteractableObject : MonoBehaviour
{
    //TODO
    //����������� ��������� � ��������� � �������, ������� ���� ������
    //����� ������� ��� ������� � ������ � ����� ��������������

    /// <summary>
    /// �������� ������ ��������
    /// </summary>
    [SerializeField] protected Animator descriptionAnim;

    /// <summary>
    /// ����� �� ���� ������������ ��������������
    /// </summary>
    protected bool _interactable = false;
    /// <summary>
    /// ���������� ����� ������������ �������
    /// </summary>
    protected GlobalKeyHandler _keyHandler;

    private void Start()
    {
        _keyHandler = GameObject.FindWithTag("Global").GetComponent<GlobalKeyHandler>();

        //��� ������� ������ ��������������
        _keyHandler.onInteractKeyDown += () =>
        {
            //���� �������������� ��������, �������� ��������������
            if (_interactable) StartInteract();
        };
        //��� ���������� ������ ��������������
        _keyHandler.onInteractKeyUp += () =>
        {
            //���� �������������� ��������, ��������� ��������������
            if (_interactable) StopInteract();
        };
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            SetInteractable(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            SetInteractable(false);
    }

    /// <summary>
    /// ���������� ��� ������� ������� ������ �������������� � �������� �������
    /// </summary>
    protected abstract void StartInteract();
    /// <summary>
    /// ���������� ��� ���������� ������� ������ �������������� ��� ��� ������ �� ��������
    /// </summary>
    protected abstract void StopInteract();

    /// <summary>
    /// ������������� �������� _interactable ������ active, ��� ������������� ��������� ��������������
    /// </summary>
    /// <param name="active"></param>
    protected void SetInteractable(bool active)
    {
        _interactable = active;

        if (!active)
            StopInteract();

        //����� �������� ��������� ��� ��������� ��������
    }
}
