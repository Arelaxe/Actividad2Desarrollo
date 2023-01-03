using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPausaSalida : MonoBehaviour
{
    [SerializeField] private GameObject ExitPanel;

    public void Jugar(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void MenuPrincipal(){
        SceneManager.LoadScene("MenuInicial");
    }

    public void Resume(){
        Time.timeScale = 1.0f;
        ExitPanel.SetActive(false);
    }

    public void Salir(){
        Debug.Log("Saliendo...");
        Application.Quit();
    }
}
