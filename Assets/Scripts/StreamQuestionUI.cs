using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ink.Runtime;
using System.Collections;
using System.Collections.Generic;

public class StreamQuestionUI : MonoBehaviour
{
    [Header("Ink — Quiz par jour")]
    public TextAsset quizJour1;
    public TextAsset quizJour2;
    public TextAsset quizJour3;
    private Story story;

    [Header("UI")]
    public GameObject questionPanel;
    public GameObject choicePanel;
    public TMP_Text questionText;
    public Button replyButton;
    public Button validateButton;

    [Header("Choices")]
    public Transform choicesContainer;
    public GameObject togglePrefab;

    [Header("Timing")]
    public float startDelay = 1f;
    public float betweenDelay = 1f;

    [Header("Timer")]
    public float questionTimeLimit = 50f;
    public Slider timerBar;
    public TMP_Text timerText;

    [Header("BDD")]
    public QuizDataSender dataSender;

    private const int ViewersPerQuestion  = 150;
    private const int SignaturesPerAnswer = 100;
    private const string TimerChoiceText  = "Temps ecoule";

    private class ChoiceData
    {
        public string text;
        public bool isCorrect;
        public Toggle toggle;
    }

    private List<ChoiceData> currentChoices = new List<ChoiceData>();
    private string lastChosenText = "";
    private Coroutine timerCoroutine;
    private bool quizFinished = false;
    private int currentQuestionNumber = 0;

    void Start()
    {
        questionPanel.SetActive(false);
        choicePanel.SetActive(false);
        if (timerBar != null) timerBar.gameObject.SetActive(false);
        replyButton.onClick.AddListener(OpenChoices);
        validateButton.onClick.AddListener(ValidateChoices);
        validateButton.gameObject.SetActive(false);
        StartCoroutine(StartQuiz());
    }

    public void StartNewDay()
    {
        quizFinished = false;
        currentQuestionNumber = 0;
        lastChosenText = "";
        currentChoices.Clear();
        story = null;
        StartCoroutine(StartQuiz());
    }

    // Appelé par DayManager quand le timer du jour expire
    // Coupe les questions en cours et marque les non répondues "Temps écoulé"
    public void ForceFinish()
    {
        if (quizFinished) return;

        Debug.Log("[Quiz] ForceFinish — arrêt brutal, questions restantes → Temps écoulé");

        StopTimer();

        // Fermer tous les panels
        questionPanel.SetActive(false);
        choicePanel.SetActive(false);
        replyButton.gameObject.SetActive(false);
        validateButton.gameObject.SetActive(false);

        if (story != null)
        {
            // Parcourir le reste du story en choisissant "Temps ecoule" à chaque question
            int safetyLimit = 20;
            while (safetyLimit-- > 0)
            {
                // Avancer le texte jusqu'aux choix
                while (story.canContinue)
                {
                    story.Continue();
                    if (story.currentChoices.Count > 0) break;
                }

                if (story.currentChoices.Count == 0) break;

                // Chercher le choix timer
                int timerIdx = -1;
                for (int i = 0; i < story.currentChoices.Count; i++)
                    if (story.currentChoices[i].text == TimerChoiceText) { timerIdx = i; break; }

                int choiceIdx = timerIdx >= 0 ? timerIdx : 0;
                story.ChooseChoiceIndex(choiceIdx);

                // Enregistrer la non-réponse
                currentQuestionNumber++;
                switch (currentQuestionNumber)
                {
                    case 1: GameState.reponse_1 = "Pas de réponse"; GameState.explication_q1 = "Temps écoulé."; break;
                    case 2: GameState.reponse_2 = "Pas de réponse"; GameState.explication_q2 = "Temps écoulé."; break;
                    case 3: GameState.reponse_3 = "Pas de réponse"; GameState.explication_q3 = "Temps écoulé."; break;
                    case 4: GameState.reponse_4 = "Pas de réponse"; GameState.explication_q4 = "Temps écoulé."; break;
                    case 5: GameState.reponse_5 = "Pas de réponse"; GameState.explication_q5 = "Temps écoulé."; break;
                    case 6: GameState.reponse_6 = "Pas de réponse"; GameState.explication_q6 = "Temps écoulé."; break;
                }
            }
        }

        FinishQuiz();
    }

    TextAsset GetCurrentDayInk()
    {
        switch (GameState.currentDay)
        {
            case 1: return quizJour1;
            case 2: return quizJour2;
            case 3: return quizJour3;
            default: return quizJour1;
        }
    }

    IEnumerator StartQuiz()
    {
        if (!GameState.CanStartQuestion()) yield break;

        TextAsset inkAsset = GetCurrentDayInk();
        if (inkAsset == null) { Debug.LogWarning("[Quiz] Ink du jour " + GameState.currentDay + " non assigné !"); yield break; }

        story = new Story(inkAsset.text);
        yield return new WaitForSeconds(startDelay);
        GameState.Set(GameMode.Question);
        LoadStep();
    }

    void LoadStep()
    {
        if (quizFinished) return;
        string currentText = "";
        if (story == null) return;

        if (!story.canContinue && story.currentChoices.Count == 0) { FinishQuiz(); return; }

        while (story.canContinue)
        {
            string line = story.Continue();
            if (!string.IsNullOrEmpty(line) && line.Trim() != lastChosenText.Trim())
                currentText += line + "\n";
            if (story.currentChoices.Count > 0) break;
        }

        lastChosenText = "";

        if (string.IsNullOrEmpty(currentText.Trim()) && story.currentChoices.Count == 0) { FinishQuiz(); return; }

        bool onlyTimer = story.currentChoices.Count == 1 && story.currentChoices[0].text == TimerChoiceText;
        if (onlyTimer) { FinishQuiz(); return; }

        questionText.text = currentText;
        questionPanel.SetActive(true);
        choicePanel.SetActive(false);
        replyButton.gameObject.SetActive(true);
        validateButton.gameObject.SetActive(false);
        StartTimer();
    }

    void StartTimer()
    {
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(RunTimer());
    }

    void StopTimer()
    {
        if (timerCoroutine != null) { StopCoroutine(timerCoroutine); timerCoroutine = null; }
        if (timerBar  != null) timerBar.gameObject.SetActive(false);
        if (timerText != null) timerText.text = "";
    }

    IEnumerator RunTimer()
    {
        float elapsed = 0f;
        if (timerBar != null)
        {
            timerBar.minValue = 0f; timerBar.maxValue = questionTimeLimit;
            timerBar.value = questionTimeLimit; timerBar.gameObject.SetActive(true);
        }
        while (elapsed < questionTimeLimit)
        {
            elapsed += Time.deltaTime;
            float remaining = questionTimeLimit - elapsed;
            if (timerBar  != null) timerBar.value = remaining;
            if (timerText != null) timerText.text  = Mathf.CeilToInt(remaining) + "s";
            yield return null;
        }
        OnTimerExpired();
    }

    void OnTimerExpired()
    {
        StopTimer();
        questionPanel.SetActive(false);
        choicePanel.SetActive(false);
        replyButton.gameObject.SetActive(false);
        validateButton.gameObject.SetActive(false);

        int timerIdx = -1;
        for (int i = 0; i < story.currentChoices.Count; i++)
            if (story.currentChoices[i].text == TimerChoiceText) { timerIdx = i; break; }
        if (timerIdx >= 0) { lastChosenText = TimerChoiceText; story.ChooseChoiceIndex(timerIdx); }

        StartCoroutine(NextStep());
    }

    void OpenChoices()
    {
        questionPanel.SetActive(false);
        replyButton.gameObject.SetActive(false);
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
            if (choice.text == TimerChoiceText) continue;

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
        StopTimer();

        List<string> checkedTexts = new List<string>();
        List<string> correctTexts = new List<string>();

        foreach (ChoiceData cd in currentChoices)
        {
            if (cd.isCorrect) correctTexts.Add(cd.text);
            if (cd.toggle != null && cd.toggle.isOn) checkedTexts.Add(cd.text);
        }

        bool allCorrectChecked = true;
        bool noWrongChecked = true;
        foreach (ChoiceData cd in currentChoices)
        {
            bool isChecked = cd.toggle != null && cd.toggle.isOn;
            if (cd.isCorrect && !isChecked) allCorrectChecked = false;
            if (!cd.isCorrect && isChecked) noWrongChecked = false;
        }

        bool isFullyCorrect = allCorrectChecked && noWrongChecked;

        GameState.AddViewers(ViewersPerQuestion);
        GameState.AddSignatures(SignaturesPerAnswer);

        int firstCheckedIndex = -1;
        for (int i = 0; i < story.currentChoices.Count; i++)
        {
            if (story.currentChoices[i].text == TimerChoiceText) continue;
            if (checkedTexts.Contains(story.currentChoices[i].text)) { firstCheckedIndex = i; break; }
        }
        if (firstCheckedIndex < 0)
            for (int i = 0; i < story.currentChoices.Count; i++)
                if (story.currentChoices[i].text != TimerChoiceText) { firstCheckedIndex = i; break; }

        string reponsesStr = checkedTexts.Count > 0 ? string.Join(", ", checkedTexts) : "Aucune réponse";
        SaveAnswerToGameState(reponsesStr, isFullyCorrect, correctTexts);

        if (firstCheckedIndex >= 0) { lastChosenText = story.currentChoices[firstCheckedIndex].text; story.ChooseChoiceIndex(firstCheckedIndex); }

        choicePanel.SetActive(false);
        validateButton.gameObject.SetActive(false);
        StartCoroutine(NextStep());
    }

    void SaveAnswerToGameState(string reponsesStr, bool isCorrect, List<string> correctTexts)
    {
        currentQuestionNumber++;
        string explication = isCorrect ? "Bonne réponse !" : "Mauvaise réponse. La bonne était : " + string.Join(", ", correctTexts);
        if (isCorrect) GameState.quizScore++;

        switch (currentQuestionNumber)
        {
            case 1: GameState.question_1 = ReadVar("question_1"); GameState.reponse_1 = reponsesStr; GameState.explication_q1 = explication; break;
            case 2: GameState.question_2 = ReadVar("question_2"); GameState.reponse_2 = reponsesStr; GameState.explication_q2 = explication; break;
            case 3: GameState.question_3 = ReadVar("question_3"); GameState.reponse_3 = reponsesStr; GameState.explication_q3 = explication; break;
            case 4: GameState.question_4 = ReadVar("question_4"); GameState.reponse_4 = reponsesStr; GameState.explication_q4 = explication; break;
            case 5: GameState.question_5 = ReadVar("question_5"); GameState.reponse_5 = reponsesStr; GameState.explication_q5 = explication; break;
            case 6: GameState.question_6 = ReadVar("question_6"); GameState.reponse_6 = reponsesStr; GameState.explication_q6 = explication; break;
        }
    }

    IEnumerator NextStep()
    {
        questionPanel.SetActive(false); choicePanel.SetActive(false);
        replyButton.gameObject.SetActive(false); validateButton.gameObject.SetActive(false);
        if (!story.canContinue && story.currentChoices.Count == 0) { FinishQuiz(); yield break; }
        yield return new WaitForSeconds(betweenDelay);
        LoadStep();
    }

    string ReadVar(string varName)
    {
        try { return story.variablesState[varName]?.ToString() ?? ""; }
        catch { return ""; }
    }

    void FinishQuiz()
    {
        if (quizFinished) return;
        quizFinished = true;
        StopTimer();

        if (story == null) { GameState.Reset(); return; }

        GameState.question_1 = ReadVar("question_1");
        GameState.question_2 = ReadVar("question_2");
        GameState.question_3 = ReadVar("question_3");
        GameState.question_4 = ReadVar("question_4");
        GameState.question_5 = ReadVar("question_5");
        GameState.question_6 = ReadVar("question_6");

        questionPanel.SetActive(false);
        choicePanel.SetActive(false);

        if (dataSender != null) dataSender.SendResults();

        story = null;
        GameState.Reset();
        Debug.Log("[Quiz] Jour " + GameState.currentDay + " terminé !");
    }
}