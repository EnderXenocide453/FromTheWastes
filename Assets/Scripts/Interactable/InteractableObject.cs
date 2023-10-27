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
    [SerializeField] protected TMPro.TMP_Text descriptionPanel;

    protected abstract string Description { get; }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            OnPlayerEnter();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            OnPlayerExit();
    }

    /// <summary>
    /// ���������� ��� ������� ������� ������ �������������� � �������� �������
    /// </summary>
    public abstract void StartInteract(Carrier carrier = null);
    /// <summary>
    /// ���������� ��� ���������� ������� ������ �������������� ��� ��� ������ �� ��������
    /// </summary>
    public abstract void StopInteract(Carrier carrier = null);

    protected virtual void OnPlayerEnter() 
    {
        ShowDescription(true);
    }
    protected virtual void OnPlayerExit()
    {
        ShowDescription(false);
    }

    /// <summary>
    /// ������������� �������� _interactable ������ active, ��� ������������� ��������� ��������������
    /// </summary>
    /// <param name="active"></param>
    protected void ShowDescription(bool active)
    {
        if (!descriptionAnim) {
            Debug.Log("�������� �������� �� ������");
        } else {
            descriptionAnim.SetBool("Show", active);
            descriptionAnim.speed = 1;
        }

        if (!descriptionPanel) {
            Debug.Log("������ �������� �� ������");
        } else {
            descriptionPanel.text = Description;
        }
    }
}
