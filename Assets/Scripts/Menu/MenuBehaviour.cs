using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuBehaviour : MonoBehaviour
{
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject controls;
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip clip;

    public void Play(){
        //audio
        SceneManager.LoadScene("Level1");
    }

    public void Controls(){
        // audio
        menu.SetActive(false);
        controls.SetActive(true);
    }

    public void Return(){
        // audio
        controls.SetActive(false);
        menu.SetActive(true);
    }

    public void Exit(){
        //audio
        Application.Quit();
    }
}
