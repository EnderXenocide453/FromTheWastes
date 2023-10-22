using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Абстрактный класс интерактивных объектов
/// </summary>
public abstract class InteractableObject : MonoBehaviour
{
    //TODO
    //Всплывающая подсказка с описанием и кнопкой, которую надо зажать
    //Вызов события или функции в начале и конце взаимодействия

    /// <summary>
    /// Аниматор панели описания
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
    /// Вызывается при нажатии игроком кнопки взаимодействия в триггере объекта
    /// </summary>
    public abstract void StartInteract(Carrier carrier = null);
    /// <summary>
    /// Вызывается при отпускании игроком кнопки взаимодействия или при выходе из триггера
    /// </summary>
    public abstract void StopInteract(Carrier carrier = null);

    /// <summary>
    /// Устанавливает значение _interactable равным active, при необходимости завершает взаимодействие
    /// </summary>
    /// <param name="active"></param>
    protected void ShowDescription(bool active)
    {
        if (!descriptionAnim) {
            Debug.Log("Анимация описания не задана");
            return;
        }

        descriptionAnim.SetBool("Show", active);
        descriptionAnim.speed = 1;
    }
}
