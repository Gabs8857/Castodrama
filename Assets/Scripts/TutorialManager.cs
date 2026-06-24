using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("UI")]
    public GameObject tutorialPanel;       // panel avec le texte d'indication
    public TMP_Text tutorialText;          // texte de l'étape courante
    public TMP_Text stepCounterText;       // ex: "1 / 6" (optionnel)
    public GameObject pressEPrompt;        // icône "Appuie sur E" (optionnel)
    public Image progressBar;             // barre de progression (optionnel)

    [Header("Références scène")]
    public DayAndNightCycle dayNightCycle; // pour désactiver le timer
    public GameObject player;

    [Header("Scène suivante")]
    public string gameSceneName = "Rivière";

    // Étape courante
    private int currentStep = 0;
    private bool stepValidated = false;
    private bool tutorialFinished = false;

    // Flags de validation par étape
    private bool playerHasMoved = false;
    private bool playerHasInteracted = false;
    private bool quizStarted = false;
    private bool quizAnswered = false;
    private bool bilanSeen = false;

    // ── ÉTAPES ───────────────────────────────────────────────────────────────
    private readonly string[] stepTexts = new string[]
    {
        "🕹️  Utilise <b>Z Q S D</b> (ou les flèches) pour te déplacer.",
        "💬  Approche-toi du personnage et appuie sur <b>E</b> pour lui parler.",
        "❓  Marche jusqu'à la zone quiz (icône au sol) pour déclencher une question.",
        "✅  Coche ta réponse puis clique sur <b>Valider</b>.",
        "🌙  Reviens voir le personnage et appuie sur <b>E</b> pour voir ton bilan.",
        "🎉  Bravo ! Tu connais les bases. Bonne aventure !",
    };

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Désactiver le timer jour/nuit pendant le tuto
        if (dayNightCycle != null)
            dayNightCycle.enabled = false;

        // Réinitialiser GameState pour le tuto
        GameState.currentDay = 1;
        GameState.isInHut = false;

        ShowStep(0);
    }

    void Update()
    {
        if (tutorialFinished) return;

        // Détection du mouvement pour l'étape 0
        if (currentStep == 0 && !playerHasMoved)
        {
            var kb = Keyboard.current;
            if (kb.wKey.isPressed || kb.aKey.isPressed ||
                kb.sKey.isPressed || kb.dKey.isPressed ||
                kb.zKey.isPressed || kb.qKey.isPressed ||
                kb.upArrowKey.isPressed || kb.downArrowKey.isPressed ||
                kb.leftArrowKey.isPressed || kb.rightArrowKey.isPressed)
            {
                playerHasMoved = true;
                ValidateCurrentStep();
            }
        }
    }

    // ── API PUBLIQUE — appelée depuis les autres scripts ─────────────────────

    // Appelé par NPCInteraction quand le joueur appuie sur E
    public void OnPlayerInteracted()
    {
        if (currentStep == 1 && !playerHasInteracted)
        {
            playerHasInteracted = true;
            ValidateCurrentStep();
        }
        // Étape 4 : retour au NPC pour le bilan
        else if (currentStep == 4 && !bilanSeen)
        {
            // On attend que le dialogue soit terminé (appelé par OnBilanDialogueDone)
        }
    }

    // Appelé par QuizZone.TriggerQuestion (ou OnTriggerEnter2D)
    public void OnQuizTriggered()
    {
        if (currentStep == 2 && !quizStarted)
        {
            quizStarted = true;
            ValidateCurrentStep();
        }
    }

    // Appelé par StreamQuestionUI.Finish()
    public void OnQuizAnswered()
    {
        if (currentStep == 3 && !quizAnswered)
        {
            quizAnswered = true;
            ValidateCurrentStep();
        }
    }

    // Appelé par DialogueManager.End() quand le dialogue de bilan est terminé
    public void OnBilanDialogueDone()
    {
        if (currentStep == 4 && !bilanSeen)
        {
            bilanSeen = true;
            ValidateCurrentStep();
        }
    }

    // ── LOGIQUE DES ÉTAPES ────────────────────────────────────────────────────

    void ValidateCurrentStep()
    {
        StartCoroutine(AdvanceStep());
    }

    IEnumerator AdvanceStep()
    {
        // Petit délai pour que le joueur voit ce qu'il s'est passé
        yield return new WaitForSeconds(0.8f);

        currentStep++;

        if (currentStep >= stepTexts.Length)
        {
            FinishTutorial();
        }
        else
        {
            ShowStep(currentStep);
        }
    }

    void ShowStep(int index)
    {
        currentStep = index;

        if (tutorialText != null)
            tutorialText.text = stepTexts[index];

        if (stepCounterText != null)
            stepCounterText.text = (index + 1) + " / " + stepTexts.Length;

        if (progressBar != null)
            progressBar.fillAmount = (float)(index + 1) / stepTexts.Length;

        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);

        // Affiche l'icône E sur les étapes où c'est pertinent
        if (pressEPrompt != null)
            pressEPrompt.SetActive(index == 1 || index == 4);

        Debug.Log("[Tuto] Étape " + (index + 1) + " : " + stepTexts[index]);
    }

    void FinishTutorial()
    {
        tutorialFinished = true;

        if (tutorialText != null)
            tutorialText.text = stepTexts[stepTexts.Length - 1];

        Debug.Log("[Tuto] Terminé ! Chargement de " + gameSceneName);
        StartCoroutine(LoadGameAfterDelay());
    }

    IEnumerator LoadGameAfterDelay()
    {
        yield return new WaitForSeconds(2.5f);

        // Réactiver le timer avant de partir
        if (dayNightCycle != null)
            dayNightCycle.enabled = true;

        SceneManager.LoadScene(gameSceneName);
    }

    // Bouton "Passer le tuto" (optionnel)
    public void SkipTutorial()
    {
        SceneManager.LoadScene(gameSceneName);
    }
}