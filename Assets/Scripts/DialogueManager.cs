using UnityEngine;
using TMPro;
using Ink.Runtime;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
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

    [Header("Ink — Dialogues (fichier unique)")]
    public TextAsset dialoguesInk;  // ← assigner Dialogues.ink ici, plus besoin de 3 slots

    [Header("Couleurs des personnages")]
    public string[] characterColors = new string[]
    {
        "Castor=#F4A261",
        "Laura Musqué=#74C0FC"
    };

    private Story story;
    private List<string> pendingLines    = new List<string>();
    private List<string> pendingSpeakers = new List<string>();
    private bool waitingForSpace = false;
    private bool choicesVisible  = false;
    private string lastChosenText = "";

    public bool dialogueFinished { get; private set; } = false;
    public bool dialogueBlocked  { get; private set; } = false;

    void Start() => dialoguePanel.SetActive(false);

    void Update()
    {
        if (waitingForSpace && !choicesVisible &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
            ShowNextLine();
    }

    // inkJSON conservé en paramètre pour compatibilité avec l'existant,
    // mais ignoré : on utilise toujours dialoguesInk + current_day.
    public void StartDialogue(TextAsset inkJSON = null)
    {
        dialogueBlocked = false;
        if (!GameState.CanStartDialogue()) { Debug.Log("Dialogue bloqué"); dialogueBlocked = true; return; }

        if (dialoguesInk == null)
        {
            Debug.LogWarning("[Dialogue] dialoguesInk non assigné dans l'Inspector !");
            dialogueBlocked = true;
            return;
        }

        dialogueFinished = false;
        pendingLines.Clear(); pendingSpeakers.Clear();
        waitingForSpace = false; choicesVisible = false; lastChosenText = "";

        GameState.Set(GameMode.Dialogue);
        story = new Story(dialoguesInk.text);

        // Injecter le jour courant pour brancher au bon dialogue
        story.variablesState["current_day"] = GameState.currentDay;

        // Variables de quiz injectées
        InjectVar("score",           GameState.quizScore);
        InjectVar("signatures_total",GameState.signatures);

        InjectVar("question_1",      GameState.question_1);
        InjectVar("reponse_1",       GameState.reponse_1);
        InjectVar("explication_q1",  GameState.explication_q1);
        InjectVar("question_2",      GameState.question_2);
        InjectVar("reponse_2",       GameState.reponse_2);
        InjectVar("explication_q2",  GameState.explication_q2);
        InjectVar("question_3",      GameState.question_3);
        InjectVar("reponse_3",       GameState.reponse_3);
        InjectVar("explication_q3",  GameState.explication_q3);
        InjectVar("question_4",      GameState.question_4);
        InjectVar("reponse_4",       GameState.reponse_4);
        InjectVar("explication_q4",  GameState.explication_q4);
        InjectVar("question_5",      GameState.question_5);
        InjectVar("reponse_5",       GameState.reponse_5);
        InjectVar("explication_q5",  GameState.explication_q5);
        InjectVar("question_6",      GameState.question_6);
        InjectVar("reponse_6",       GameState.reponse_6);
        InjectVar("explication_q6",  GameState.explication_q6);
        InjectVar("question_7",      GameState.question_7);
        InjectVar("reponse_7",       GameState.reponse_7);
        InjectVar("explication_q7",  GameState.explication_q7);
        InjectVar("question_8",      GameState.question_8);
        InjectVar("reponse_8",       GameState.reponse_8);
        InjectVar("explication_q8",  GameState.explication_q8);
        InjectVar("question_9",      GameState.question_9);
        InjectVar("reponse_9",       GameState.reponse_9);
        InjectVar("explication_q9",  GameState.explication_q9);
        InjectVar("question_10",     GameState.question_10);
        InjectVar("reponse_10",      GameState.reponse_10);
        InjectVar("explication_q10", GameState.explication_q10);
        InjectVar("question_11",     GameState.question_11);
        InjectVar("reponse_11",      GameState.reponse_11);
        InjectVar("explication_q11", GameState.explication_q11);
        InjectVar("question_12",     GameState.question_12);
        InjectVar("reponse_12",      GameState.reponse_12);
        InjectVar("explication_q12", GameState.explication_q12);
        InjectVar("question_13",     GameState.question_13);
        InjectVar("reponse_13",      GameState.reponse_13);
        InjectVar("explication_q13", GameState.explication_q13);
        InjectVar("question_14",     GameState.question_14);
        InjectVar("reponse_14",      GameState.reponse_14);
        InjectVar("explication_q14", GameState.explication_q14);
        InjectVar("question_15",     GameState.question_15);
        InjectVar("reponse_15",      GameState.reponse_15);
        InjectVar("explication_q15", GameState.explication_q15);
        InjectVar("question_16",     GameState.question_16);
        InjectVar("reponse_16",      GameState.reponse_16);
        InjectVar("explication_q16", GameState.explication_q16);
        InjectVar("question_17",     GameState.question_17);
        InjectVar("reponse_17",      GameState.reponse_17);
        InjectVar("explication_q17", GameState.explication_q17);

        dialoguePanel.SetActive(true);
        LoadNextLines();
    }

    void InjectVar(string varName, object value)
    {
        try { story.variablesState[varName] = value; }
        catch { /* variable absente du ink, on ignore */ }
    }

    void LoadNextLines()
    {
        pendingLines.Clear(); pendingSpeakers.Clear();
        if (story == null) return;

        while (story.canContinue)
        {
            string line    = story.Continue();
            string speaker = GetSpeakerFromTags();
            if (!string.IsNullOrWhiteSpace(line) && line.Trim() != lastChosenText.Trim())
            { pendingLines.Add(line.Trim()); pendingSpeakers.Add(speaker); }
            if (story.currentChoices.Count > 0) break;
        }

        lastChosenText = "";
        if (pendingLines.Count > 0)              ShowNextLine();
        else if (story.currentChoices.Count > 0) ShowChoices();
        else                                     End();
    }

    void ShowNextLine()
    {
        if (pendingLines.Count > 0)
        {
            string line = pendingLines[0]; string speaker = pendingSpeakers[0];
            pendingLines.RemoveAt(0); pendingSpeakers.RemoveAt(0);
            dialogueText.text = FormatLine(line);
            UpdatePortrait(speaker);
            waitingForSpace = true; choicesVisible = false;
        }
        else
        {
            if (story.currentChoices.Count > 0) ShowChoices();
            else if (story.canContinue)          LoadNextLines();
            else                                 End();
        }
    }

    string GetSpeakerFromTags()
    {
        if (story == null) return "";
        foreach (string tag in story.currentTags)
            if (tag.Trim().StartsWith("speaker:"))
                return tag.Trim().Substring("speaker:".Length).Trim();
        return "";
    }

    void UpdatePortrait(string speakerName)
    {
        if (portraitImage == null) return;
        if (string.IsNullOrEmpty(speakerName)) { portraitImage.gameObject.SetActive(false); return; }
        for (int i = 0; i < characterNames.Length; i++)
            if (characterNames[i] == speakerName)
            { portraitImage.sprite = characterSprites[i]; portraitImage.gameObject.SetActive(true); return; }
        portraitImage.gameObject.SetActive(false);
    }

    string FormatLine(string line)
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

    string GetColorForCharacter(string name)
    {
        foreach (string entry in characterColors)
        {
            string[] parts = entry.Split('=');
            if (parts.Length == 2 && parts[0].Trim() == name.Trim()) return parts[1].Trim();
        }
        return "#FFFFFF";
    }

    void ShowChoices()
    {
        choicesVisible = true; waitingForSpace = false;
        foreach (Transform c in choicesContainer) Destroy(c.gameObject);
        foreach (Choice choice in story.currentChoices)
        {
            GameObject btn = Instantiate(choicePrefab, choicesContainer);
            btn.GetComponentInChildren<TMP_Text>().text = choice.text;
            int i = choice.index; string choiceText = choice.text;
            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                lastChosenText = choiceText; story.ChooseChoiceIndex(i);
                foreach (Transform c in choicesContainer) Destroy(c.gameObject);
                choicesVisible = false; LoadNextLines();
            });
        }
    }

    void End()
    {
        dialogueText.text = "";
        if (portraitImage != null) portraitImage.gameObject.SetActive(false);
        dialoguePanel.SetActive(false);
        GameState.Reset(); story = null;
        pendingLines.Clear(); pendingSpeakers.Clear();
        waitingForSpace = false; choicesVisible = false; lastChosenText = "";
        // Hook tuto
        if (TutorialManager.Instance != null)
            TutorialManager.Instance.OnBilanDialogueDone();

        dialogueFinished = true;
        Debug.Log("Dialogue terminé");
    }
}