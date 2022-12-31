using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuBehaviour : MonoBehaviour
{
    public void Jugar(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void Controles(){
        SceneManager.LoadScene("Controles");
    }

    public void Volver(){
        SceneManager.LoadScene("MenuInicial");
    }

    public void Salir(){
        Debug.Log("Saliendo...");
        Application.Quit();
    }
}
