using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject pausePanel; // le panel gris avec les boutons

    private bool isPaused = false;

    void Start()
    {
        // S'assurer que le panel est caché au démarrage
        pausePanel.SetActive(false);
    }

    void Update()
    {
        // Echap pour ouvrir/fermer le menu pause
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    void Pause()
    {
        // Bloquer le jeu seulement si on est en mode Free
        // (pas pendant un dialogue ou une question)
        if (GameState.Mode == GameMode.Question || GameState.Mode == GameMode.Dialogue)
            return;

        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f; // met le jeu en pause
        Debug.Log("[PauseMenu] Pause");
    }

    public void Resume()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f; // reprend le jeu
        Debug.Log("[PauseMenu] Reprise");
    }

    public void RestartFromMenu()
    {
        Time.timeScale = 1f; // remettre à 1 avant de changer de scène
        GameState.Reset();

        // Recharge la scène du menu principal
        SceneManager.LoadScene("Menu"); // remplace par le nom exact de ta scène menu
        Debug.Log("[PauseMenu] Retour au menu");
    }
}