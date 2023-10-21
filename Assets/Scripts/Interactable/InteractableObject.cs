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

    /// <summary>
    /// Может ли быть осуществлено взаимодействие
    /// </summary>
    protected bool _interactable = false;
    /// <summary>
    /// Глобальный класс отслеживания нажатий
    /// </summary>
    protected GlobalKeyHandler _keyHandler;

    private void Start()
    {
        _keyHandler = GameObject.FindWithTag("Global").GetComponent<GlobalKeyHandler>();

        //При нажатии кнопки взаимодействия
        _keyHandler.onInteractKeyDown += () =>
        {
            //Если взаимодействие возможно, начинаем взаимодействие
            if (_interactable) StartInteract();
        };
        //При отпускании кнопки взаимодействия
        _keyHandler.onInteractKeyUp += () =>
        {
            //Если взаимодействие возможно, завершаем взаимодействие
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
    /// Вызывается при нажатии игроком кнопки взаимодействия в триггере объекта
    /// </summary>
    protected abstract void StartInteract();
    /// <summary>
    /// Вызывается при отпускании игроком кнопки взаимодействия или при выходе из триггера
    /// </summary>
    protected abstract void StopInteract();

    /// <summary>
    /// Устанавливает значение _interactable равным active, при необходимости завершает взаимодействие
    /// </summary>
    /// <param name="active"></param>
    protected void SetInteractable(bool active)
    {
        _interactable = active;

        if (!active)
            StopInteract();

        //Через аниматор открываем или закрываем описание
    }
}
