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
            Debug.Log("[Quiz] Impossible de lancer, mode = " + GameState.Mode);
            return;
        }

        if (quizInk == null) { Debug.LogWarning("[Quiz] quizInk non assigné !"); return; }

        onDone = onFinished;
        currentGlobalIndex = globalIndex;
        currentChoices.Clear();

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

    public void StartNewDay() => ForceFinish();

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
        List<string> checkedTexts = new List<string>();
        List<string> correctTexts = new List<string>();
        foreach (ChoiceData cd in currentChoices)
        {
            if (cd.isCorrect) correctTexts.Add(cd.text);
            if (cd.toggle != null && cd.toggle.isOn) checkedTexts.Add(cd.text);
        }

        ChoiceData chosen = null;
        foreach (ChoiceData cd in currentChoices)
            if (cd.toggle != null && cd.toggle.isOn) { chosen = cd; break; }
        if (chosen == null && currentChoices.Count > 0) chosen = currentChoices[0];
        if (chosen == null) return;

        if (checkedTexts.Count == 0) checkedTexts.Add(chosen.text);

        int choiceIndex = -1;
        for (int i = 0; i < story.currentChoices.Count; i++)
            if (story.currentChoices[i].text == chosen.text) { choiceIndex = i; break; }
        if (choiceIndex < 0) choiceIndex = 0;

        story.ChooseChoiceIndex(choiceIndex);
        choicePanel.SetActive(false);
        validateButton.gameObject.SetActive(false);

        // Avancer le texte et capturer le contenu suivant (Drama éventuel)
        string nextText = "";
        while (story.canContinue)
        {
            string line = story.Continue();
            if (!string.IsNullOrWhiteSpace(line))
                nextText += line.Trim() + "\n";
            if (story.currentChoices.Count > 0) break;
        }

        // S'il y a d'autres choix (Drama) → afficher le nouvel écran
        if (story.currentChoices.Count > 0)
        {
            questionText.text = nextText.Trim();
            questionPanel.SetActive(true);
            choicePanel.SetActive(false);
            replyButton.gameObject.SetActive(true);
            validateButton.gameObject.SetActive(false);
            return;
        }

        // Fin — on sauvegarde la réponse du DERNIER écran validé
        bool allCorrectChecked = correctTexts.Count > 0
            && checkedTexts.TrueForAll(t => correctTexts.Contains(t))
            && correctTexts.TrueForAll(t => checkedTexts.Contains(t));

        GameState.AddViewers(ViewersPerQuestion);
        GameState.AddSignatures(SignaturesPerAnswer);

        string reponsesStr = string.Join(", ", checkedTexts);
        SaveAnswerToGameState(reponsesStr, allCorrectChecked, correctTexts);

        Finish();
    }

    // Sauvegarde dans les variables RELATIVES (reponse_1 à reponse_6)
    // en convertissant le globalIndex en index relatif au jour courant
    void SaveAnswerToGameState(string reponsesStr, bool isCorrect, List<string> correctTexts)
    {
        string explication = isCorrect
            ? "Bonne reponse !"
            : "Mauvaise reponse. La bonne etait : " + string.Join(", ", correctTexts);

        if (isCorrect) GameState.quizScore++;

        // Convertit l'index global en index relatif au jour (1-5 ou 1-6)
        // Jour 1 : globalIndex 1-5  → relatif 1-5
        // Jour 2 : globalIndex 6-11 → relatif 1-6
        // Jour 3 : globalIndex 12-17 → relatif 1-6
        int[] offsets = new int[] { 0, 0, 5, 11 }; // index 0 inutilisé
        int dayOffset = GameState.currentDay >= 1 && GameState.currentDay <= 3
            ? offsets[GameState.currentDay] : 0;
        int relativeIndex = currentGlobalIndex - dayOffset;

        Debug.Log("[Quiz] SaveAnswer globalIndex=" + currentGlobalIndex
            + " jour=" + GameState.currentDay
            + " relativeIndex=" + relativeIndex
            + " reponse=" + reponsesStr);

        switch (relativeIndex)
        {
            case 1: GameState.reponse_1 = reponsesStr; GameState.explication_q1 = explication; break;
            case 2: GameState.reponse_2 = reponsesStr; GameState.explication_q2 = explication; break;
            case 3: GameState.reponse_3 = reponsesStr; GameState.explication_q3 = explication; break;
            case 4: GameState.reponse_4 = reponsesStr; GameState.explication_q4 = explication; break;
            case 5: GameState.reponse_5 = reponsesStr; GameState.explication_q5 = explication; break;
            case 6: GameState.reponse_6 = reponsesStr; GameState.explication_q6 = explication; break;
            default:
                Debug.LogWarning("[Quiz] relativeIndex hors range : " + relativeIndex
                    + " (globalIndex=" + currentGlobalIndex + ", jour=" + GameState.currentDay + ")");
                break;
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
        Debug.Log("[Quiz] Question " + currentGlobalIndex + " terminée.");

        onDone?.Invoke();
        onDone = null;
    }
}