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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            ShowDescription(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            ShowDescription(false);
    }

    /// <summary>
    /// ���������� ��� ������� ������� ������ �������������� � �������� �������
    /// </summary>
    public abstract void StartInteract(Carrier carrier = null);
    /// <summary>
    /// ���������� ��� ���������� ������� ������ �������������� ��� ��� ������ �� ��������
    /// </summary>
    public abstract void StopInteract(Carrier carrier = null);

    /// <summary>
    /// ������������� �������� _interactable ������ active, ��� ������������� ��������� ��������������
    /// </summary>
    /// <param name="active"></param>
    protected void ShowDescription(bool active)
    {
        if (!descriptionAnim) {
            Debug.Log("�������� �������� �� ������");
            return;
        }

        descriptionAnim.SetBool("Show", active);
        descriptionAnim.speed = 1;
    }
}
