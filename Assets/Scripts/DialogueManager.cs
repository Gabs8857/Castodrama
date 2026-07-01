using UnityEngine;
using TMPro;
using Ink.Runtime;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    // =========================================================================
    // RÉFÉRENCES UI
    // =========================================================================
    [Header("UI")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;

    [Header("Portrait")]
    public Image portraitImage;
    public string[] characterNames;
    public Sprite[] characterSprites;

    [Header("Choices")]
    public Transform choicesContainer;
    public GameObject choicePrefab;

    // =========================================================================
    // FICHIERS INK
    // =========================================================================
    [Header("Ink Files")]
    public TextAsset dialoguesInk; // Fichier par défaut (peut être vide)
    public TextAsset globalsJSON;  // Variables globales (ex: globals.ink)

    [Header("Character Colors")]
    public string[] characterColors = new string[]
    {
        "Castor=#F4A261",
        "Laura Musqué=#74C0FC",
        "Socrate=#FFFFFF",
        "Junior=#FFD700"
    };

    // =========================================================================
    // SINGLETON ET VARIABLES
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

            // Initialiser les variables globales
            if (globalsJSON != null)
            {
                DialogueVariables = new DialogueVariables(globalsJSON);
            }
            else
            {
                Debug.LogWarning("[DialogueManager] globalsJSON non assigné ! Les variables ne seront pas persistées.");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        dialoguePanel?.SetActive(false);
    }

    void Update()
    {
        if (waitingForSpace && !choicesVisible && Keyboard.current?.spaceKey.wasPressedThisFrame == true)
        {
            ShowNextLine();
        }
    }

    // =========================================================================
    // DÉMARRAGE DES DIALOGUES
    // =========================================================================
    public void EnterDialogueMode(TextAsset inkJSON)
    {
        StartDialogue(inkJSON);
    }

    public void StartDialogue(TextAsset inkJSON = null)
    {
        dialogueBlocked = false;

        // Vérifier si le dialogue est autorisé
        if (!GameState.CanStartDialogue())
        {
            Debug.Log("[DialogueManager] Dialogue bloqué par GameState");
            dialogueBlocked = true;
            return;
        }

        // Utiliser le fichier Ink du trigger, ou le fichier par défaut
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

        // Synchroniser avec les variables globales
        if (DialogueVariables != null)
        {
            DialogueVariables.StartListening(story);
        }

        // Injecter les variables de jeu
        InjectVar("current_day", GameState.currentDay);
        InjectVar("score", GameState.quizScore);
        InjectVar("signatures_total", GameState.signatures);

        // Variables de quiz (version explicite pour éviter les erreurs)
        InjectVar("question_1", GameState.question_1);
        InjectVar("reponse_1", GameState.reponse_1);
        InjectVar("explication_q1", GameState.explication_q1);
        InjectVar("question_2", GameState.question_2);
        InjectVar("reponse_2", GameState.reponse_2);
        InjectVar("explication_q2", GameState.explication_q2);
        InjectVar("question_3", GameState.question_3);
        InjectVar("reponse_3", GameState.reponse_3);
        InjectVar("explication_q3", GameState.explication_q3);

        // Afficher le panneau
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }
        else
        {
            Debug.LogError("[DialogueManager] dialoguePanel non assigné !");
        }

        LoadNextLines();
    }

    private void InjectVar(string varName, object value)
    {
        if (value == null || story == null) return;

        try
        {
            // Conversion automatique des types C# vers Ink
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
                // Tentative de cast direct pour les autres types
                story.variablesState[varName] = (Ink.Runtime.Object)value;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[DialogueManager] Impossible d'injecter '{varName}': {e.Message}");
        }
    }

    // =========================================================================
    // GESTION DES LIGNES ET CHOIX
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

        // Sauvegarder les variables
        if (DialogueVariables != null && story != null)
        {
            DialogueVariables.StopListening(story);
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