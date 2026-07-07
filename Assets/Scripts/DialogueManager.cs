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
    public GameObject mapScreen; // Assigne ton GameObject de carte ici

    // =========================================================================
    // INK FILES
    // =========================================================================
    [Header("Ink Configuration")]
    public TextAsset dialoguesInk; // Peut être vide (on utilise startKnot)
    public TextAsset globalsJSON;  // Fichier avec les variables globales

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
            {
                DialogueVariables = new DialogueVariables(globalsJSON);
            }
            else
            {
                Debug.LogWarning("[DialogueManager] globalsJSON non assigné !");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    void Update()
    {
        SyncInputModeVar();

        if (waitingForSpace && !choicesVisible && InputHelper.SubmitPressed())
        {
            ShowNextLine();
        }

        // Fermeture manuelle de la carte avec ÉCHAP
        if (mapScreen != null && mapScreen.activeSelf && InputHelper.PausePressed())
        {
            mapScreen.SetActive(false);
        }
    }

    // =========================================================================
    // DÉMARRAGE DES DIALOGUES (AVEC SUPPORT DES KNOTS)
    // =========================================================================
    public void EnterDialogueMode(TextAsset inkJSON)
    {
        StartDialogue(inkJSON);
    }

    /// <summary>
    /// Démarre un dialogue avec un fichier Ink et un Knot de départ optionnel
    /// </summary>
    /// <param name="inkJSON">Fichier Ink à utiliser</param>
    /// <param name="startKnot">Nom du Knot de départ (ex: "tutodebut")</param>
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

        // Démarrer la Story
        GameState.Set(GameMode.Dialogue);
        story = new Story(dialogueToUse.text);

        // ✅ SUPPORT DES KNOTS : Si un Knot de départ est spécifié
        if (!string.IsNullOrEmpty(startKnot))
        {
            story.ChoosePathString(startKnot);
            Debug.Log($"[DialogueManager] Démarrage au Knot: {startKnot}");
        }

        // Synchroniser avec les variables globales
        if (DialogueVariables != null)
        {
            DialogueVariables.StartListening(story);
        }

        // Injecter les variables de jeu
        InjectVar("current_day", GameState.currentDay);
        InjectVar("score", GameState.quizScore);
        InjectVar("signatures_total", GameState.signatures);
        InjectVar("gamepad", InputHelper.IsGamepadPreferred());

        // Variables de quiz
        for (int i = 1; i <= 17; i++)
        {
            InjectVar($"question_{i}", typeof(GameState).GetField($"question_{i}")?.GetValue(null));
            InjectVar($"reponse_{i}", typeof(GameState).GetField($"reponse_{i}")?.GetValue(null));
            InjectVar($"explication_q{i}", typeof(GameState).GetField($"explication_q{i}")?.GetValue(null));
        }

        // Afficher le panneau
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        LoadNextLines();
    }

    private void SyncInputModeVar()
    {
        if (story == null) return;

        InjectVar("gamepad", InputHelper.IsGamepadPreferred());
    }

    // =========================================================================
    // GESTION DES VARIABLES INK
    // =========================================================================
    private void InjectVar(string varName, object value)
    {
        if (value == null || story == null) return;

        try
        {
            if (value is bool boolValue)
            {
                story.variablesState[varName] = new BoolValue(boolValue);
            }
            else if (value is int intValue)
            {
                story.variablesState[varName] = new IntValue(intValue);
            }
            else if (value is float floatValue)
            {
                story.variablesState[varName] = new FloatValue(floatValue);
            }
            else if (value is string stringValue)
            {
                story.variablesState[varName] = new StringValue(stringValue);
            }
            else
            {
                story.variablesState[varName] = (Ink.Runtime.Object)value;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[DialogueManager] Erreur injection variable '{varName}': {e.Message}");
        }
    }

    public void SetBoolVariable(string varName, bool value)
    {
        if (story == null) return;

        try
        {
            story.variablesState[varName] = new BoolValue(value);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[DialogueManager] Erreur injection bool '{varName}': {e.Message}");
        }
    }

    public bool IsDialogueOpen => story != null;

    public void FinishDialogueNow()
    {
        if (story == null) return;

        End();
    }

    // =========================================================================
    // GESTION DES LIGNES ET KNOTS
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
            }

            if (story.currentChoices.Count > 0) break;
        }

        lastChosenText = "";

        if (pendingLines.Count > 0)
        {
            ShowNextLine();
        }
        else if (story.currentChoices.Count > 0)
        {
            ShowChoices();
        }
        else
        {
            End();
        }
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
            {
                dialogueText.text = FormatLine(line);
            }

            UpdatePortrait(speaker);
            waitingForSpace = true;
            choicesVisible = false;

            // Vérifier les tags spéciaux (carte, etc.)
            CheckSpecialTags();
        }
        else
        {
            if (story.currentChoices.Count > 0)
            {
                ShowChoices();
            }
            else if (story.canContinue)
            {
                LoadNextLines();
            }
            else
            {
                End();
            }
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

        if (choicesContainer != null)
        {
            foreach (Transform child in choicesContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (Choice choice in story.currentChoices)
            {
                GameObject btn = Instantiate(choicePrefab, choicesContainer);
                TMP_Text choiceText = btn.GetComponentInChildren<TMP_Text>();

                if (choiceText != null)
                {
                    choiceText.text = choice.text;
                }

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
                        {
                            Destroy(c.gameObject);
                        }

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
    }

    // =========================================================================
    // FIN DE DIALOGUE
    // =========================================================================
    private void End()
    {
        if (dialogueText != null)
        {
            dialogueText.text = "";
        }

        if (portraitImage != null)
        {
            portraitImage.gameObject.SetActive(false);
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        if (DialogueVariables != null && story != null)
        {
            DialogueVariables.StopListening(story);
        }

        if (choicesNavigator != null)
        {
            choicesNavigator.gameObject.SetActive(false);
        }

        GameState.Reset();
        story = null;

        dialogueFinished = true;
        Debug.Log("[DialogueManager] Dialogue terminé");
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
            {
                return tag.Trim().Substring("speaker:".Length).Trim();
            }
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
            {
                return parts[1].Trim();
            }
        }
        return "#FFFFFF";
    }
}