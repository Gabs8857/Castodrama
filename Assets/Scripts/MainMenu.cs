using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MainMenu : MonoBehaviour
{
    [Header("Panel tuto")]
    public GameObject tutoPanel;

    void Start()
    {
        Debug.Log("[MainMenu] Script démarré — Appuie sur F9 pour debug");

        // Nettoyage des PlayerPrefs debug au démarrage du menu
        PlayerPrefs.DeleteKey("debug_mode");
        PlayerPrefs.DeleteKey("player_id");
        PlayerPrefs.Save();
        Debug.Log("[MainMenu] PlayerPrefs nettoyés");

        if (tutoPanel != null) tutoPanel.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current.f9Key.wasPressedThisFrame)
        {
            Debug.Log("[MainMenu] F9 appuyé !");
            StartDebugGame();
        }
    }

    public void PlayGame()
    {
        Debug.Log("[MainMenu] Mode NORMAL lancé");
        PlayerPrefs.SetInt("debug_mode", 0);
        PlayerPrefs.Save();
        if (tutoPanel != null)
            tutoPanel.SetActive(true);
        else
            SceneManager.LoadSceneAsync("TUTO");
    }

    public void GoToTuto()
    {
        PlayerPrefs.SetInt("debug_mode", 0);
        PlayerPrefs.Save();
        SceneManager.LoadSceneAsync("TUTO");
    }

    public void GoToGame()
    {
        PlayerPrefs.SetInt("debug_mode", 0);
        PlayerPrefs.Save();
        SceneManager.LoadSceneAsync("TUTO");
    }

    public void CloseTutoPanel()
    {
        if (tutoPanel != null) tutoPanel.SetActive(false);
    }

    public void StartDebugGame()
    {
        Debug.Log("[MainMenu] 🔴 MODE DEBUG ACTIVÉ !");
        PlayerPrefs.SetInt("debug_mode", 1);
        PlayerPrefs.SetString("player_id", "DEBUG");
        PlayerPrefs.Save();
        SceneManager.LoadSceneAsync("Rivière");
    }

    public void QuitGame()
    {
        Debug.Log("[MainMenu] Quitting game");
        Application.Quit();
    }
}