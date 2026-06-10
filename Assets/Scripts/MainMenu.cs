using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // ← AJOUTE CETTE LIGNE

public class MainMenu : MonoBehaviour
{
    void Start()
    {
        Debug.Log("[MainMenu] Script démarré — Appuie sur F9 pour debug");
    }

    void Update()
    {
        // Utilise le New Input System au lieu de Input.GetKeyDown
        if (Keyboard.current.f9Key.wasPressedThisFrame)
        {
            Debug.Log("[MainMenu] F9 appuyé !");
            StartDebugGame();
        }
    }

    public void PLayGame()
    {
        Debug.Log("[MainMenu] Mode NORMAL lancé");
        PlayerPrefs.SetInt("debug_mode", 0);
        PlayerPrefs.Save();
        SceneManager.LoadSceneAsync("Rivière");
    }

    public void StartDebugGame()
    {
        Debug.Log("[MainMenu] 🔴 MODE DEBUG ACTIVÉ !");
        PlayerPrefs.SetInt("debug_mode", 1);
        PlayerPrefs.SetString("player_id", "DEBUG");
        PlayerPrefs.Save();

        Debug.Log("[MainMenu] debug_mode = " + PlayerPrefs.GetInt("debug_mode", 0));
        Debug.Log("[MainMenu] player_id = " + PlayerPrefs.GetString("player_id", "none"));
        
        SceneManager.LoadSceneAsync("Rivière");
    }

    public void QuitGame()
    {
        Debug.Log("[MainMenu] Quitting game");
        Application.Quit();
    }
}