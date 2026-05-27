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
    private List<string> pendingLines = new List<string>();
    private bool waitingForSpace = false;
    private bool choicesVisible = false;
    private string lastChosenText = ""; // mémorise le choix cliqué pour l'ignorer

    public bool dialogueFinished { get; private set; } = false;
    public bool dialogueBlocked { get; private set; } = false;

    void Start()
    {
        dialoguePanel.SetActive(false);
    }

    void Update()
    {
        if (waitingForSpace && !choicesVisible &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ShowNextLine();
        }
    }

    public void StartDialogue(TextAsset inkJSON)
    {
        dialogueBlocked = false;

        if (!GameState.CanStartDialogue())
        {
            Debug.Log("Dialogue bloqué");
            dialogueBlocked = true;
            return;
        }

        dialogueFinished = false;
        pendingLines.Clear();
        waitingForSpace = false;
        choicesVisible = false;
        lastChosenText = "";

        GameState.Set(GameMode.Dialogue);

        story = new Story(inkJSON.text);

        story.variablesState["score"] = GameState.quizScore;
        story.variablesState["firstAnswer"] = GameState.firstAnswer;
        story.variablesState["secondAnswer"] = GameState.secondAnswer;
        story.variablesState["q1_explanation"] = GameState.q1Explanation;
        story.variablesState["q2_explanation"] = GameState.q2Explanation;

        dialoguePanel.SetActive(true);

        LoadNextLines();
    }

    void LoadNextLines()
    {
        pendingLines.Clear();

        if (story == null) return;

        while (story.canContinue)
        {
            string line = story.Continue();

            // Ignorer la ligne si c'est le texte du choix qu'on vient de sélectionner
            if (!string.IsNullOrWhiteSpace(line) && line.Trim() != lastChosenText.Trim())
                pendingLines.Add(line.Trim());

            if (story.currentChoices.Count > 0)
                break;
        }

        lastChosenText = ""; // reset après usage

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

    void ShowNextLine()
    {
        if (pendingLines.Count > 0)
        {
            string line = pendingLines[0];
            pendingLines.RemoveAt(0);

            dialogueText.text = FormatLine(line);
            waitingForSpace = true;
            choicesVisible = false;
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

    string FormatLine(string line)
    {
        int colonIndex = line.IndexOf(" : ");
        if (colonIndex > 0)
        {
            string name = line.Substring(0, colonIndex).Trim();
            string text = line.Substring(colonIndex + 3).Trim();
            string color = GetColorForCharacter(name);
            return $"<color={color}><b>{name}</b></color>\n{text}";
        }
        return line;
    }

    string GetColorForCharacter(string name)
    {
        foreach (string entry in characterColors)
        {
            string[] parts = entry.Split('=');
            if (parts.Length == 2 && parts[0].Trim() == name.Trim())
                return parts[1].Trim();
        }
        return "#FFFFFF";
    }

    void ShowChoices()
    {
        choicesVisible = true;
        waitingForSpace = false;

        foreach (Transform c in choicesContainer)
            Destroy(c.gameObject);

        foreach (Choice choice in story.currentChoices)
        {
            GameObject btn = Instantiate(choicePrefab, choicesContainer);
            btn.GetComponentInChildren<TMP_Text>().text = choice.text;

            int i = choice.index;
            string choiceText = choice.text;

            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                lastChosenText = choiceText; // mémorisé pour être ignoré au prochain LoadNextLines

                story.ChooseChoiceIndex(i);

                foreach (Transform c in choicesContainer)
                    Destroy(c.gameObject);

                choicesVisible = false;
                LoadNextLines();
            });
        }
    }

    void End()
    {
        dialogueText.text = ""; // vider le texte pour ne pas laisser de résidu
        dialoguePanel.SetActive(false);
        GameState.Reset();
        story = null;
        pendingLines.Clear();
        waitingForSpace = false;
        choicesVisible = false;
        lastChosenText = "";

        dialogueFinished = true;

        Debug.Log("Dialogue terminé");
    }
}