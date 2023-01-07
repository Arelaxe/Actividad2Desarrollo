using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuBehaviour : MonoBehaviour
{
    // Switch menus between Initial Menu and Controls Menu
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject controls;
    
    // Pause menu 
    [SerializeField] private GameObject ExitPanel;

    // Player
    [SerializeField] private GameObject player;

    // Audio 
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip clip;
    [SerializeField] private AudioClip pop;

    public void Audio(){
        source.PlayOneShot(clip);
    }

    public void AudioHover(){
        source.PlayOneShot(pop);
    }

    public void Play(){
        SceneManager.LoadScene("Level1");
    }

    public void Resume(){
        Time.timeScale = 1.0f;
        ExitPanel.SetActive(false);
        player.GetComponent<PlayerController>().SetPaused(false);
    }

    public void Controls(){
        menu.SetActive(false);
        controls.SetActive(true);
    }

    public void Menu(){
        SceneManager.LoadScene("MenuInicial");
    }

    public void Return(){
        controls.SetActive(false);
        menu.SetActive(true);
    }

    public void Exit(){
        Application.Quit();
    }
}
