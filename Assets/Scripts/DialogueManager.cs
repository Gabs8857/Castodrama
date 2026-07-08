using UnityEngine;
using TMPro;
using Ink.Runtime;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    // =========================================================================
    // REFERENCES UI
    // =========================================================================
    [Header("UI Elements")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;

    [Header("Portrait Settings")]
    public Image portraitImage;
    public string[] characterNames;
    public Sprite[] characterSprites;

    [Header("Choice System")]
    public Transform choicesContainer;
    public GameObject choicePrefab;

    [Header("Gamepad")]
    [SerializeField] private GamepadButtonListNavigator choicesNavigator;

    [Header("Map System")]
    public GameObject mapScreen;

    // =========================================================================
    // INK FILES
    // =========================================================================
    [Header("Ink Configuration")]
    public TextAsset dialoguesInk;
    public TextAsset globalsJSON;

    [Header("Character Colors")]
    public string[] characterColors = new string[]
    {
        "Castor=#F4A261",
        "Laura Musqué=#74C0FC",
        "Socrate=#FFFFFF",
        "Junior=#FFD700"
    };

    // =========================================================================
    // SINGLETON & VARIABLES
    // =========================================================================
    public static DialogueManager Instance { get; private set; }
    public DialogueVariables DialogueVariables { get; private set; }

    private Story story;
    private List<string> pendingLines = new List<string>();
    private List<string> pendingSpeakers = new List<string>();
    private bool waitingForSpace = false;
    private bool choicesVisible = false;
    private string lastChosenText = "";

    public bool dialogueFinished { get; private set; } = false;
    public bool dialogueBlocked { get; private set; } = false;
    private TopDownHunger tutohaloHungerSystem;
    private bool tutohaloHungerLockActive = false;

    // Pour le debug gamepad : valeur injectée au dernier StartDialogue
    private bool lastInjectedGamepad = false;

    // =========================================================================
    // INITIALISATION
    // =========================================================================
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (globalsJSON != null)
                DialogueVariables = new DialogueVariables(globalsJSON);
            else
                Debug.LogWarning("[DialogueManager] globalsJSON non assigné !");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    void Update()
    {
        SyncInputModeVar();

        if (waitingForSpace && !choicesVisible && InputHelper.SubmitPressed())
            ShowNextLine();

        if (mapScreen != null && mapScreen.activeSelf && InputHelper.PausePressed())
            mapScreen.SetActive(false);
    }

    // =========================================================================
    // DÉMARRAGE DES DIALOGUES
    // =========================================================================
    public void EnterDialogueMode(TextAsset inkJSON) => StartDialogue(inkJSON);

    /// <summary>
    /// Démarre un dialogue Ink avec knot optionnel.
    /// IMPORTANT : gamepad est injecté AVANT ChoosePathString pour que les
    /// conditions {gamepad:...} dans Ink soient évaluées avec la bonne valeur.
    /// </summary>
    public void StartDialogue(TextAsset inkJSON = null, string startKnot = null)
    {
        dialogueBlocked = false;

        if (!GameState.CanStartDialogue())
        {
            Debug.Log("[DialogueManager] Dialogue bloqué par GameState");
            dialogueBlocked = true;
            return;
        }

        TextAsset dialogueToUse = inkJSON ?? dialoguesInk;
        if (dialogueToUse == null)
        {
            Debug.LogError("[DialogueManager] Aucun fichier Ink disponible !");
            dialogueBlocked = true;
            return;
        }

        // Réinitialisation
        dialogueFinished = false;
        pendingLines.Clear();
        pendingSpeakers.Clear();
        waitingForSpace = false;
        choicesVisible = false;
        lastChosenText = "";

        GameState.Set(GameMode.Dialogue);
        story = new Story(dialogueToUse.text);

        // ✅ FIX : Injecter gamepad ET les variables AVANT ChoosePathString
        // pour que les conditions {gamepad:...} soient évaluées correctement
        // dès le premier Continue() — même si le knot fait ->autreKnot en chaîne.
        bool isGamepad = InputHelper.IsGamepadPreferred();
        lastInjectedGamepad = isGamepad;

        Debug.Log("═══════════════════════════════════════");
        Debug.Log($"[DialogueManager] StartDialogue → knot='{startKnot ?? "(root)"}'");
        Debug.Log($"[DialogueManager] 🎮 gamepad = {isGamepad}");
        Debug.Log($"[DialogueManager]    Gamepad.current != null : {Gamepad.current != null}");
        Debug.Log($"[DialogueManager]    IsGamepadPreferred()    : {isGamepad}");
        Debug.Log("═══════════════════════════════════════");

        // 🔑 Variables injectées AVANT ChoosePathString
        InjectVar("gamepad", isGamepad);
        InjectVar("current_day", GameState.currentDay);
        InjectVar("score", GameState.quizScore);
        InjectVar("signatures_total", GameState.signatures);

        for (int i = 1; i <= 17; i++)
        {
            InjectVar($"question_{i}", typeof(GameState).GetField($"question_{i}")?.GetValue(null));
            InjectVar($"reponse_{i}", typeof(GameState).GetField($"reponse_{i}")?.GetValue(null));
            InjectVar($"explication_q{i}", typeof(GameState).GetField($"explication_q{i}")?.GetValue(null));
        }

        // Tutohalo spécial
        if (!string.IsNullOrEmpty(startKnot) &&
            string.Equals(startKnot.Trim(), "tutohalo", System.StringComparison.OrdinalIgnoreCase))
            ApplyTutohaloHungerLock();

        // ChoosePathString APRÈS l'injection
        if (!string.IsNullOrEmpty(startKnot))
        {
            try
            {
                story.ChoosePathString(startKnot);
                Debug.Log($"[DialogueManager] ✓ ChoosePathString('{startKnot}') OK");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[DialogueManager] ❌ ChoosePathString('{startKnot}') échoué : {e.Message}");
            }
        }

        // Synchroniser avec les variables globales APRÈS ChoosePathString
        if (DialogueVariables != null)
            DialogueVariables.StartListening(story);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        LoadNextLines();
    }

    /// <summary>
    /// Synchro gamepad chaque frame — met à jour la variable Ink si elle change.
    /// Log uniquement quand la valeur change (pas de spam).
    /// </summary>
    private void SyncInputModeVar()
    {
        if (story == null) return;

        bool isGamepad = InputHelper.IsGamepadPreferred();

        if (isGamepad != lastInjectedGamepad)
        {
            Debug.Log($"[DialogueManager] 🎮 SyncInputModeVar : gamepad {lastInjectedGamepad} → {isGamepad} (device changé en cours de dialogue)");
            lastInjectedGamepad = isGamepad;
            InjectVar("gamepad", isGamepad);
        }
    }

    // =========================================================================
    // GESTION DES VARIABLES INK
    // =========================================================================
    private void InjectVar(string varName, object value)
    {
        if (value == null || story == null) return;

        // Vérifie que la variable existe dans la Story avant d'injecter
        // (évite le spam de warnings pour les variables de quiz absentes du fichier tuto)
        try
        {
            var _ = story.variablesState[varName];
        }
        catch
        {
            // Variable non déclarée dans ce fichier Ink — on ignore silencieusement
            return;
        }

        try
        {
            // ✅ Ink attend les types C# natifs (bool, int, float, string),
            // PAS les wrappers BoolValue/IntValue/etc. — c'est ce qui causait
            // "Invalid value passed to VariableState"
            if (value is bool boolValue)
            {
                story.variablesState[varName] = boolValue;
                if (varName == "gamepad")
                    Debug.Log($"[DialogueManager] ✓ InjectVar('gamepad', {boolValue}) — OK");
            }
            else if (value is int intValue)
                story.variablesState[varName] = intValue;
            else if (value is float floatValue)
                story.variablesState[varName] = floatValue;
            else if (value is string stringValue)
                story.variablesState[varName] = stringValue;
            else
                story.variablesState[varName] = (Ink.Runtime.Object)value;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[DialogueManager] ⚠ Erreur injection '{varName}': {e.Message}");
        }
    }

    public void SetBoolVariable(string varName, bool value)
    {
        if (story == null) return;

        try
        {
            story.variablesState[varName] = value; // bool natif, pas BoolValue
            Debug.Log($"[DialogueManager] SetBoolVariable('{varName}', {value})");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[DialogueManager] Erreur SetBoolVariable '{varName}': {e.Message}");
        }
    }

    /// <summary>
    /// Lit une variable globale Ink (persistante via DialogueVariables),
    /// utilisable même quand aucun dialogue n'est en cours.
    /// Utile pour conditionner des triggers/objets dans la scène (ex: "fintuto").
    /// </summary>
    public bool GetGlobalBool(string varName)
    {
        if (DialogueVariables == null || DialogueVariables.variables == null)
        {
            Debug.LogWarning("[DialogueManager] DialogueVariables non initialisé.");
            return false;
        }

        if (!DialogueVariables.variables.TryGetValue(varName, out Ink.Runtime.Object val))
        {
            Debug.LogWarning($"[DialogueManager] Variable '{varName}' introuvable dans DialogueVariables.");
            return false;
        }

        if (val is Ink.Runtime.BoolValue boolVal)
            return boolVal.value;

        Debug.LogWarning($"[DialogueManager] Variable '{varName}' n'est pas un bool (type: {val.GetType()}).");
        return false;
    }

    public bool IsDialogueOpen => story != null;

    public void FinishDialogueNow()
    {
        if (story == null) return;
        Debug.Log("[DialogueManager] FinishDialogueNow() appelé");
        End();
    }

    private void ApplyTutohaloHungerLock()
    {
        if (tutohaloHungerLockActive) return;

        if (tutohaloHungerSystem == null)
            tutohaloHungerSystem = FindObjectOfType<TopDownHunger>();

        if (tutohaloHungerSystem == null)
        {
            Debug.LogWarning("[DialogueManager] TopDownHunger introuvable pour tutohalo.");
            return;
        }

        tutohaloHungerSystem.SetHunger(30f);
        tutohaloHungerSystem.SetHungerDrainPaused(true);
        tutohaloHungerLockActive = true;
        Debug.Log("[DialogueManager] tutohalo → faim fixée à 30 et décrue bloquée.");
    }

    private void ReleaseTutohaloHungerLock()
    {
        if (!tutohaloHungerLockActive || tutohaloHungerSystem == null)
        {
            tutohaloHungerLockActive = false;
            return;
        }

        tutohaloHungerSystem.SetHungerDrainPaused(false);
        tutohaloHungerLockActive = false;
        Debug.Log("[DialogueManager] tutohalo → décrue faim réactivée.");
    }

    // =========================================================================
    // GESTION DES LIGNES
    // =========================================================================
    private void LoadNextLines()
    {
        pendingLines.Clear();
        pendingSpeakers.Clear();

        if (story == null) return;

        while (story.canContinue)
        {
            string line = story.Continue();
            string speaker = GetSpeakerFromTags();

            if (!string.IsNullOrWhiteSpace(line) && line.Trim() != lastChosenText.Trim())
            {
                pendingLines.Add(line.Trim());
                pendingSpeakers.Add(speaker);

                // 🔍 Debug : affiche chaque ligne lue avec la valeur gamepad au moment de la lecture
                Debug.Log($"[DialogueManager] 📖 Ligne lue (gamepad={lastInjectedGamepad}): \"{line.Trim()}\"");
            }

            if (story.currentChoices.Count > 0) break;
        }

        lastChosenText = "";

        if (pendingLines.Count > 0)
            ShowNextLine();
        else if (story.currentChoices.Count > 0)
            ShowChoices();
        else
            End();
    }

    private void ShowNextLine()
    {
        if (pendingLines.Count > 0)
        {
            string line = pendingLines[0];
            string speaker = pendingSpeakers[0];

            pendingLines.RemoveAt(0);
            pendingSpeakers.RemoveAt(0);

            if (dialogueText != null)
                dialogueText.text = FormatLine(line);

            UpdatePortrait(speaker);
            waitingForSpace = true;
            choicesVisible = false;
            CheckSpecialTags();
        }
        else
        {
            if (story.currentChoices.Count > 0)
                ShowChoices();
            else if (story.canContinue)
                LoadNextLines();
            else
                End();
        }
    }

    private void CheckSpecialTags()
    {
        if (story == null || mapScreen == null) return;

        foreach (string tag in story.currentTags)
        {
            string cleanTag = tag.Trim().ToLower();
            if (cleanTag == "show_map")
            {
                mapScreen.SetActive(true);
                Debug.Log("[DialogueManager] Carte affichée");
            }
            else if (cleanTag == "hide_map")
            {
                mapScreen.SetActive(false);
                Debug.Log("[DialogueManager] Carte masquée");
            }
        }
    }

    private void ShowChoices()
    {
        choicesVisible = true;
        waitingForSpace = false;

        if (choicesContainer == null) return;

        foreach (Transform child in choicesContainer)
            Destroy(child.gameObject);

        foreach (Choice choice in story.currentChoices)
        {
            GameObject btn = Instantiate(choicePrefab, choicesContainer);
            TMP_Text choiceText = btn.GetComponentInChildren<TMP_Text>();
            if (choiceText != null) choiceText.text = choice.text;

            int choiceIndex = choice.index;
            string chosenText = choice.text;

            Button button = btn.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() =>
                {
                    lastChosenText = chosenText;
                    story.ChooseChoiceIndex(choiceIndex);

                    foreach (Transform c in choicesContainer)
                        Destroy(c.gameObject);

                    choicesVisible = false;
                    LoadNextLines();
                });
            }
        }

        if (choicesNavigator != null)
        {
            choicesNavigator.gameObject.SetActive(true);
            choicesNavigator.ResetSelection();
        }
    }

    // =========================================================================
    // FIN DE DIALOGUE
    // =========================================================================
    private void End()
    {
        Debug.Log("[DialogueManager] End() → dialogue terminé");

        if (dialogueText != null) dialogueText.text = "";
        if (portraitImage != null) portraitImage.gameObject.SetActive(false);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        if (DialogueVariables != null && story != null)
            DialogueVariables.StopListening(story);

        if (choicesNavigator != null)
            choicesNavigator.gameObject.SetActive(false);

        GameState.Reset();
        ReleaseTutohaloHungerLock();
        story = null;
        dialogueFinished = true;

        Debug.Log("[DialogueManager] ✓ Dialogue terminé — GameState reset");
    }

    // =========================================================================
    // UTILITAIRES
    // =========================================================================
    private string GetSpeakerFromTags()
    {
        if (story == null) return "";

        foreach (string tag in story.currentTags)
        {
            if (tag.Trim().StartsWith("speaker:"))
                return tag.Trim().Substring("speaker:".Length).Trim();
        }
        return "";
    }

    private void UpdatePortrait(string speakerName)
    {
        if (portraitImage == null) return;

        if (string.IsNullOrEmpty(speakerName))
        {
            portraitImage.gameObject.SetActive(false);
            return;
        }

        for (int i = 0; i < characterNames.Length; i++)
        {
            if (characterNames[i] == speakerName)
            {
                portraitImage.sprite = characterSprites[i];
                portraitImage.gameObject.SetActive(true);
                return;
            }
        }

        portraitImage.gameObject.SetActive(false);
    }

    private string FormatLine(string line)
    {
        int colonIndex = line.IndexOf(" : ");
        if (colonIndex > 0)
        {
            string name = line.Substring(0, colonIndex).Trim();
            string text = line.Substring(colonIndex + 3).Trim();
            return $"<color={GetColorForCharacter(name)}><b>{name}</b></color>\n{text}";
        }
        return line;
    }

    private string GetColorForCharacter(string name)
    {
        foreach (string entry in characterColors)
        {
            string[] parts = entry.Split('=');
            if (parts.Length == 2 && parts[0].Trim() == name.Trim())
                return parts[1].Trim();
        }
        return "#FFFFFF";
    }
}