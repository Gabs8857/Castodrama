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
    public GameObject tutorialPanel;
    public TMP_Text tutorialText;
    public TMP_Text stepCounterText;
    public GameObject pressEPrompt;
    public Image progressBar;

    [Header("Références scène")]
    public DayAndNightCycle dayNightCycle;
    public GameObject player;

    [Header("Ink — Tuto Retour")]
    [Tooltip("Fichier Ink du tuto (le même que les autres knots)")]
    public TextAsset tutoInkJSON;

    [Header("Scène suivante")]
    public string gameSceneName = "Rivière";

    [Header("Debug")]
    [Tooltip("Active/désactive tous les logs de debug de ce script")]
    public bool debugLogs = true;

    private int currentStep = 0;
    private bool tutorialFinished = false;

    private bool playerHasMoved = false;
    private bool playerHasInteracted = false;
    private bool quizStarted = false;
    private bool quizAnswered = false;
    private bool bilanSeen = false;
    private bool playerHasEaten = false;       // ← nouveau flag
    private bool tutoRetourLaunched = false;   // ← évite de relancer plusieurs fois

    private readonly string[] stepTexts = new string[]
    {
        "🕹️  Utilise <b>Z Q S D</b> (ou les flèches) pour te déplacer.",
        "💬  Approche-toi du personnage et appuie sur <b>E</b> pour lui parler.",
        "❓  Marche jusqu'à la zone quiz (icône au sol) pour déclencher une question.",
        "✅  Coche ta réponse puis clique sur <b>Valider</b>.",
        "🌙  Reviens voir le personnage et appuie sur <b>E</b> pour voir ton bilan.",
        "🎉  Bravo ! Tu connais les bases. Bonne aventure !",
    };

    // Petit helper pour pas répéter "if (debugLogs) Debug.Log" partout
    void Log(string msg)
    {
        if (debugLogs)
            Debug.Log("[TutorialManager] " + msg);
    }

    void LogWarn(string msg)
    {
        if (debugLogs)
            Debug.LogWarning("[TutorialManager] " + msg);
    }

    void Awake()
    {
        Instance = this;
        Log("Awake() → Instance assignée.");
    }

    void Start()
    {
        Log("Start() → Initialisation du tuto.");

        if (dayNightCycle != null)
        {
            dayNightCycle.enabled = false;
            Log("dayNightCycle désactivé pour la durée du tuto.");
        }
        else
        {
            LogWarn("dayNightCycle non assigné dans l'Inspector.");
        }

        if (tutorialPanel == null) LogWarn("tutorialPanel non assigné.");
        if (tutorialText == null) LogWarn("tutorialText non assigné.");
        if (tutoInkJSON == null) LogWarn("tutoInkJSON non assigné (nécessaire pour 'tutoretour').");

        GameState.currentDay = 1;
        GameState.isInHut = false;

        ShowStep(0);
    }

    void Update()
    {
        if (tutorialFinished) return;

        if (currentStep == 0 && !playerHasMoved)
        {
            var kb = Keyboard.current;
            if (kb == null)
            {
                LogWarn("Keyboard.current est NULL — pas de clavier détecté.");
                return;
            }

            if (kb.wKey.isPressed || kb.aKey.isPressed ||
                kb.sKey.isPressed || kb.dKey.isPressed ||
                kb.zKey.isPressed || kb.qKey.isPressed ||
                kb.upArrowKey.isPressed || kb.downArrowKey.isPressed ||
                kb.leftArrowKey.isPressed || kb.rightArrowKey.isPressed)
            {
                playerHasMoved = true;
                Log("Étape 0 validée → joueur a bougé.");
                ValidateCurrentStep();
            }
        }
    }

    // ── API PUBLIQUE ──────────────────────────────────────────────────────────

    public void OnPlayerInteracted()
    {
        Log($"OnPlayerInteracted() appelé (currentStep={currentStep}, déjà fait={playerHasInteracted}).");

        if (currentStep == 1 && !playerHasInteracted)
        {
            playerHasInteracted = true;
            Log("Étape 1 validée → interaction joueur détectée.");
            ValidateCurrentStep();
        }
    }

    public void OnQuizTriggered()
    {
        Log($"OnQuizTriggered() appelé (currentStep={currentStep}, déjà fait={quizStarted}).");

        if (currentStep == 2 && !quizStarted)
        {
            quizStarted = true;
            Log("Étape 2 validée → quiz déclenché.");
            ValidateCurrentStep();
        }
    }

    public void OnQuizAnswered()
    {
        Log($"OnQuizAnswered() appelé (currentStep={currentStep}, déjà fait={quizAnswered}).");

        if (currentStep == 3 && !quizAnswered)
        {
            quizAnswered = true;
            Log("Étape 3 validée → quiz répondu.");
            ValidateCurrentStep();
        }
    }

    public void OnBilanDialogueDone()
    {
        Log($"OnBilanDialogueDone() appelé (currentStep={currentStep}, déjà fait={bilanSeen}).");

        if (currentStep == 4 && !bilanSeen)
        {
            bilanSeen = true;
            Log("Étape 4 validée → bilan vu.");
            ValidateCurrentStep();
        }
    }

    /// <summary>
    /// Appelé par FoodItem quand le joueur mange.
    /// Lance automatiquement le knot "tutoretour" dans le dialogue Ink.
    /// </summary>
    public void OnPlayerAte()
    {
        Log($"OnPlayerAte() appelé (playerHasEaten={playerHasEaten}, tutoRetourLaunched={tutoRetourLaunched}).");

        if (playerHasEaten || tutoRetourLaunched)
        {
            Log("OnPlayerAte() ignoré → déjà déclenché précédemment.");
            return;
        }

        playerHasEaten = true;
        tutoRetourLaunched = true;

        Log("🍃 Joueur a mangé → tentative de lancement de 'tutoretour'.");

        if (DialogueManager.Instance != null && tutoInkJSON != null)
        {
            DialogueManager.Instance.StartDialogue(tutoInkJSON, "tutoretour");
            Log("Dialogue 'tutoretour' lancé avec succès.");
        }
        else
        {
            if (DialogueManager.Instance == null)
                LogWarn("DialogueManager.Instance est NULL — impossible de lancer tutoretour.");
            if (tutoInkJSON == null)
                LogWarn("tutoInkJSON non assigné dans l'Inspector — glisse ton fichier Ink ici.");
        }
    }

    public void OnTutorialActionCompleted(string completionFlag, int requiredStep)
    {
        Log($"OnTutorialActionCompleted('{completionFlag}', requiredStep={requiredStep}) appelé (currentStep={currentStep}, tutorialFinished={tutorialFinished}).");

        if (tutorialFinished || currentStep != requiredStep)
        {
            Log("OnTutorialActionCompleted() ignoré → étape actuelle ne correspond pas ou tuto déjà fini.");
            return;
        }

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.SetBoolVariable(completionFlag, true);
            Log($"Variable Ink '{completionFlag}' mise à true.");

            if (DialogueManager.Instance.IsDialogueOpen)
            {
                DialogueManager.Instance.FinishDialogueNow();
                Log("Dialogue en cours fermé de force (FinishDialogueNow).");
            }
        }
        else
        {
            LogWarn("DialogueManager.Instance est NULL dans OnTutorialActionCompleted.");
        }

        ValidateCurrentStep();
    }

    // ── LOGIQUE DES ÉTAPES ────────────────────────────────────────────────────

    void ValidateCurrentStep()
    {
        Log($"ValidateCurrentStep() → démarrage de la coroutine AdvanceStep (step actuel={currentStep}).");
        StartCoroutine(AdvanceStep());
    }

    IEnumerator AdvanceStep()
    {
        yield return new WaitForSeconds(0.8f);

        currentStep++;
        Log($"AdvanceStep() → passage à l'étape {currentStep}.");

        if (currentStep >= stepTexts.Length)
            FinishTutorial();
        else
            ShowStep(currentStep);
    }

    void ShowStep(int index)
    {
        currentStep = index;

        string displayText = GetStepText(index);

        if (tutorialText != null)
            tutorialText.text = displayText;

        if (stepCounterText != null)
            stepCounterText.text = (index + 1) + " / " + stepTexts.Length;

        if (progressBar != null)
            progressBar.fillAmount = (float)(index + 1) / stepTexts.Length;

        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);

        if (pressEPrompt != null)
            pressEPrompt.SetActive(index == 1 || index == 4);

        Log("Étape " + (index + 1) + " affichée : " + displayText);
    }

    string GetStepText(int index)
    {
        if (!InputHelper.IsGamepadPreferred())
            return stepTexts[index];

        switch (index)
        {
            case 0:
                return "🕹️  Utilise le stick gauche pour te déplacer.";
            case 1:
                return "💬  Approche-toi du personnage et appuie sur <b>A</b> pour lui parler.";
            case 4:
                return "🌙  Reviens voir le personnage et appuie sur <b>A</b> pour voir ton bilan.";
            default:
                return stepTexts[index];
        }
    }

    void FinishTutorial()
    {
        tutorialFinished = true;

        if (tutorialText != null)
            tutorialText.text = stepTexts[stepTexts.Length - 1];

        Log("Tuto terminé ! Chargement de la scène : " + gameSceneName);
        StartCoroutine(LoadGameAfterDelay());
    }

    IEnumerator LoadGameAfterDelay()
    {
        Log("LoadGameAfterDelay() → attente de 2.5s avant chargement de scène.");
        yield return new WaitForSeconds(2.5f);

        if (dayNightCycle != null)
        {
            dayNightCycle.enabled = true;
            Log("dayNightCycle réactivé.");
        }

        Log("Chargement de la scène : " + gameSceneName);
        SceneManager.LoadScene(gameSceneName);
    }

    public void SkipTutorial()
    {
        Log("SkipTutorial() appelé → chargement direct de : " + gameSceneName);
        SceneManager.LoadScene(gameSceneName);
    }
}