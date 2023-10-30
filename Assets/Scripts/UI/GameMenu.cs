using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenu : MonoBehaviour
{
    [SerializeField] private Transform menu;

    private bool _paused;

    void Start()
    {
        GlobalValues.handler.onPauseKeyDown += TogglePause;
    }

    public void TogglePause()
    {
        _paused = !_paused;
        menu.gameObject.SetActive(_paused);
        Time.timeScale = _paused ? 0 : 1;
    }

    public void Restart()
    {
        GlobalValues.newGame = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
