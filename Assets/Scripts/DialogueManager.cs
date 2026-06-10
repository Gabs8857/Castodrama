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

    public void StartDialogue(TextAsset inkJSON)
    {
        dialogueBlocked = false;
        if (!GameState.CanStartDialogue()) { Debug.Log("Dialogue bloqué"); dialogueBlocked = true; return; }

        dialogueFinished = false;
        pendingLines.Clear(); pendingSpeakers.Clear();
        waitingForSpace = false; choicesVisible = false; lastChosenText = "";

        GameState.Set(GameMode.Dialogue);
        story = new Story(inkJSON.text);

        // Injection avec toutes les variables question
        story.variablesState["score"]          = GameState.quizScore;
        story.variablesState["question_1"]     = GameState.question_1;
        story.variablesState["reponse_1"]      = GameState.reponse_1;
        story.variablesState["explication_q1"] = GameState.explication_q1;

        dialoguePanel.SetActive(true);
        LoadNextLines();
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
        dialogueFinished = true;
        Debug.Log("Dialogue terminé");
    }
}