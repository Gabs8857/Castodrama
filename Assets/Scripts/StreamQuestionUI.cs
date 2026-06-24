using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ink.Runtime;
using System.Collections;
using System.Collections.Generic;

public class StreamQuestionUI : MonoBehaviour
{
    [Header("Ink — Quiz (fichier unique)")]
    public TextAsset quizInk;

    [Header("UI")]
    public GameObject questionPanel;
    public GameObject choicePanel;
    public TMP_Text questionText;
    public TMP_Text choiceQuestionText;
    public Button replyButton;
    public Button validateButton;

    [Header("Choices")]
    public Transform choicesContainer;
    public GameObject togglePrefab;

    [Header("BDD")]
    public QuizDataSender dataSender;

    private const int ViewersPerQuestion  = 150;
    private const int SignaturesPerAnswer = 100;

    private class ChoiceData
    {
        public string text;
        public bool isCorrect;
        public Toggle toggle;
    }

    private Story story;
    private List<ChoiceData> currentChoices = new List<ChoiceData>();
    private int currentGlobalIndex = 0;
    private System.Action onDone;

    // Mémorise les choix du premier écran pour la sauvegarde finale
    // (utile si Drama fait une deuxième passe de choix)
    private List<ChoiceData> firstChoices = new List<ChoiceData>();
    private bool firstChoiceDone = false;

    void Start()
    {
        questionPanel.SetActive(false);
        choicePanel.SetActive(false);
        replyButton.onClick.AddListener(OpenChoices);
        validateButton.onClick.AddListener(ValidateChoices);
        validateButton.gameObject.SetActive(false);
    }

    public void TriggerQuestion(string knotName, int globalIndex, System.Action onFinished)
    {
        if (!GameState.CanStartQuestion())
        {
            Debug.Log("[Quiz] Impossible de lancer une question, mode = " + GameState.Mode);
            return;
        }

        if (quizInk == null) { Debug.LogWarning("[Quiz] quizInk non assigne !"); return; }

        onDone = onFinished;
        currentGlobalIndex = globalIndex;
        currentChoices.Clear();
        firstChoices.Clear();
        firstChoiceDone = false;

        story = new Story(quizInk.text);
        story.variablesState["current_day"] = GameState.currentDay;
        story.ChoosePathString(knotName);

        GameState.Set(GameMode.Question);
        ShowQuestion();
    }

    public void ForceFinish()
    {
        questionPanel.SetActive(false);
        choicePanel.SetActive(false);
        replyButton.gameObject.SetActive(false);
        validateButton.gameObject.SetActive(false);
        story = null;
        GameState.Reset();
    }

    public void StartNewDay()
    {
        ForceFinish();
    }

    // -------------------------------------------------------

    void ShowQuestion()
    {
        if (story == null) return;

        string currentText = "";
        while (story.canContinue)
        {
            string line = story.Continue();
            if (!string.IsNullOrWhiteSpace(line))
                currentText += line.Trim() + "\n";
            if (story.currentChoices.Count > 0) break;
        }

        if (string.IsNullOrWhiteSpace(currentText) && story.currentChoices.Count == 0)
        {
            Finish(); return;
        }

        questionText.text = currentText.Trim();
        questionPanel.SetActive(true);
        choicePanel.SetActive(false);
        replyButton.gameObject.SetActive(true);
        validateButton.gameObject.SetActive(false);
    }

    void OpenChoices()
    {
        questionPanel.SetActive(false);
        replyButton.gameObject.SetActive(false);
        if (choiceQuestionText != null) choiceQuestionText.text = questionText.text;
        choicePanel.SetActive(true);
        validateButton.gameObject.SetActive(true);
        BuildToggles();
    }

    void BuildToggles()
    {
        List<GameObject> toDestroy = new List<GameObject>();
        foreach (Transform c in choicesContainer) if (c != null) toDestroy.Add(c.gameObject);
        foreach (GameObject go in toDestroy) if (go != null) Destroy(go);
        currentChoices.Clear();

        foreach (Choice choice in story.currentChoices)
        {
            bool isCorrect = false;
            foreach (string tag in choice.tags ?? new List<string>())
                if (tag.Trim() == "correct") { isCorrect = true; break; }

            GameObject obj = Instantiate(togglePrefab, choicesContainer);
            Toggle toggle = obj.GetComponent<Toggle>();
            TMP_Text label = obj.GetComponentInChildren<TMP_Text>(true);
            if (label != null) label.text = choice.text;
            if (toggle != null) toggle.isOn = false;

            currentChoices.Add(new ChoiceData { text = choice.text, isCorrect = isCorrect, toggle = toggle });
        }
    }

    void ValidateChoices()
    {
        // Trouver le choix selectionne
        ChoiceData chosen = null;
        foreach (ChoiceData cd in currentChoices)
            if (cd.toggle != null && cd.toggle.isOn) { chosen = cd; break; }
        if (chosen == null && currentChoices.Count > 0) chosen = currentChoices[0];
        if (chosen == null) return;

        // Memoriser le premier ecran de choix pour la sauvegarde (avant Drama)
        if (!firstChoiceDone)
        {
            firstChoices = new List<ChoiceData>(currentChoices);
            firstChoiceDone = true;
        }

        // Avancer le story avec ce choix
        int choiceIndex = -1;
        for (int i = 0; i < story.currentChoices.Count; i++)
            if (story.currentChoices[i].text == chosen.text) { choiceIndex = i; break; }
        if (choiceIndex < 0) return;

        story.ChooseChoiceIndex(choiceIndex);

        choicePanel.SetActive(false);
        validateButton.gameObject.SetActive(false);

        // Si le story a encore du contenu apres ce choix (ex: knot Drama),
        // on affiche la suite sans sauvegarder encore.
        if (story.canContinue || story.currentChoices.Count > 0)
        {
            ShowQuestion();
            return;
        }

        // Plus de contenu : c'est la reponse finale.
        // On evalue sur les choix du dernier ecran (Drama ou premier ecran).
        List<string> checkedTexts = new List<string>();
        List<string> correctTexts = new List<string>();
        foreach (ChoiceData cd in currentChoices)
        {
            if (cd.isCorrect) correctTexts.Add(cd.text);
            if (cd.toggle != null && cd.toggle.isOn) checkedTexts.Add(cd.text);
        }
        if (checkedTexts.Count == 0) checkedTexts.Add(chosen.text);

        bool allCorrectChecked = correctTexts.Count > 0
            && checkedTexts.TrueForAll(t => correctTexts.Contains(t))
            && correctTexts.TrueForAll(t => checkedTexts.Contains(t));

        GameState.AddViewers(ViewersPerQuestion);
        GameState.AddSignatures(SignaturesPerAnswer);

        string reponsesStr = string.Join(", ", checkedTexts);
        SaveAnswerToGameState(reponsesStr, allCorrectChecked, correctTexts);

        Finish();
    }

    void SaveAnswerToGameState(string reponsesStr, bool isCorrect, List<string> correctTexts)
    {
        string explication = isCorrect
            ? "Bonne reponse !"
            : "Mauvaise reponse. La bonne etait : " + string.Join(", ", correctTexts);

        if (isCorrect) GameState.quizScore++;

        string qText = ReadVar("question_" + currentGlobalIndex);

        switch (currentGlobalIndex)
        {
            case 1:  GameState.question_1  = qText; GameState.reponse_1  = reponsesStr; GameState.explication_q1  = explication; break;
            case 2:  GameState.question_2  = qText; GameState.reponse_2  = reponsesStr; GameState.explication_q2  = explication; break;
            case 3:  GameState.question_3  = qText; GameState.reponse_3  = reponsesStr; GameState.explication_q3  = explication; break;
            case 4:  GameState.question_4  = qText; GameState.reponse_4  = reponsesStr; GameState.explication_q4  = explication; break;
            case 5:  GameState.question_5  = qText; GameState.reponse_5  = reponsesStr; GameState.explication_q5  = explication; break;
            case 6:  GameState.question_6  = qText; GameState.reponse_6  = reponsesStr; GameState.explication_q6  = explication; break;
            case 7:  GameState.question_7  = qText; GameState.reponse_7  = reponsesStr; GameState.explication_q7  = explication; break;
            case 8:  GameState.question_8  = qText; GameState.reponse_8  = reponsesStr; GameState.explication_q8  = explication; break;
            case 9:  GameState.question_9  = qText; GameState.reponse_9  = reponsesStr; GameState.explication_q9  = explication; break;
            case 10: GameState.question_10 = qText; GameState.reponse_10 = reponsesStr; GameState.explication_q10 = explication; break;
            case 11: GameState.question_11 = qText; GameState.reponse_11 = reponsesStr; GameState.explication_q11 = explication; break;
            case 12: GameState.question_12 = qText; GameState.reponse_12 = reponsesStr; GameState.explication_q12 = explication; break;
            case 13: GameState.question_13 = qText; GameState.reponse_13 = reponsesStr; GameState.explication_q13 = explication; break;
            case 14: GameState.question_14 = qText; GameState.reponse_14 = reponsesStr; GameState.explication_q14 = explication; break;
            case 15: GameState.question_15 = qText; GameState.reponse_15 = reponsesStr; GameState.explication_q15 = explication; break;
            case 16: GameState.question_16 = qText; GameState.reponse_16 = reponsesStr; GameState.explication_q16 = explication; break;
            case 17: GameState.question_17 = qText; GameState.reponse_17 = reponsesStr; GameState.explication_q17 = explication; break;
        }
    }

    string ReadVar(string varName)
    {
        try { return story.variablesState[varName]?.ToString() ?? ""; }
        catch { return ""; }
    }

    void Finish()
    {
        questionPanel.SetActive(false);
        choicePanel.SetActive(false);
        story = null;
        GameState.Reset();
        Debug.Log("[Quiz] Question " + currentGlobalIndex + " terminee.");

        if (dataSender != null) dataSender.SendResults();

        // Hook tuto
        if (TutorialManager.Instance != null)
            TutorialManager.Instance.OnQuizAnswered();

        onDone?.Invoke();
        onDone = null;
    }
}