using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class CreditsManager : MonoBehaviour
{
    [Header("Crédits")]
    [TextArea(5, 20)]
    public string creditsText = "Castor Drama\n\nUn jeu éducatif sur le castor européen\n\n— Développé par —\nTon Nom\n\n— Merci d'avoir joué —";

    public TMP_Text creditsLabel;    // TMP_Text qui affiche le texte
    public float scrollSpeed = 50f;  // vitesse de défilement (pixels/sec)
    public float delayBeforeScroll = 2f; // attente avant que ça commence

    [Header("Navigation")]
    public string menuSceneName = "Menu"; // retour au menu

    private RectTransform rectTransform;
    private bool scrolling = false;

    void Start()
    {
        if (creditsLabel != null)
        {
            creditsLabel.text = creditsText;
            rectTransform = creditsLabel.GetComponent<RectTransform>();
        }

        StartCoroutine(StartScroll());
    }

    void Update()
    {
        // Echap ou Espace pour retourner au menu
        if (InputHelper.PausePressed() || InputHelper.SubmitPressed())
        {
            GoToMenu();
        }

        // Défilement automatique
        if (scrolling && rectTransform != null)
        {
            rectTransform.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;

            // Si le texte est sorti de l'écran → retour menu automatique
            if (rectTransform.anchoredPosition.y > rectTransform.rect.height + Screen.height)
                GoToMenu();
        }
    }

    IEnumerator StartScroll()
    {
        yield return new WaitForSeconds(delayBeforeScroll);
        scrolling = true;
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene(menuSceneName);
    }
}