using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ink.Runtime;
using System.Collections;

public class StreamQuestionUI : MonoBehaviour
{
    [Header("Ink")]
    public TextAsset inkJSON;
    private Story story;

    [Header("UI")]
    public GameObject questionPanel;
    public GameObject choicePanel;
    public TMP_Text questionText;
    public Button replyButton;

    [Header("Choices")]
    public Transform choicesContainer;
    public GameObject choicePrefab;

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
    private const string TimerChoiceText  = "Temps écoulé";

    private string lastChosenText = "";
    private Coroutine timerCoroutine;
    private bool quizFinished = false;

    void Start()
    {
        questionPanel.SetActive(false);
        choicePanel.SetActive(false);
        if (timerBar != null) timerBar.gameObject.SetActive(false);
        replyButton.onClick.AddListener(OpenChoices);
        StartCoroutine(StartQuiz());
    }

    IEnumerator StartQuiz()
    {
        if (!GameState.CanStartQuestion()) yield break;
        story = new Story(inkJSON.text);

        // Log toutes les variables disponibles dans le ink pour diagnostic
        Debug.Log("[Quiz] Variables disponibles dans le ink :");
        foreach (string varName in story.variablesState)
            Debug.Log("  → " + varName);

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

        questionText.text = currentText;
        questionPanel.SetActive(true);
        choicePanel.SetActive(false);
        replyButton.gameObject.SetActive(true);
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
        StartCoroutine(SkipToNext());
    }

    IEnumerator SkipToNext()
    {
        int timerChoiceIndex = -1;
        for (int i = 0; i < story.currentChoices.Count; i++)
            if (story.currentChoices[i].text == TimerChoiceText) { timerChoiceIndex = i; break; }
        if (timerChoiceIndex >= 0) { lastChosenText = TimerChoiceText; story.ChooseChoiceIndex(timerChoiceIndex); }
        yield return new WaitForSeconds(betweenDelay);
        if (!story.canContinue && story.currentChoices.Count == 0) FinishQuiz();
        else LoadStep();
    }

    void OpenChoices()
    {
        questionPanel.SetActive(false);
        replyButton.gameObject.SetActive(false);
        choicePanel.SetActive(true);
        BuildChoices();
    }

    void BuildChoices()
    {
        foreach (Transform c in choicesContainer) Destroy(c.gameObject);
        foreach (Choice choice in story.currentChoices)
        {
            if (choice.text == TimerChoiceText) continue;
            GameObject btn = Instantiate(choicePrefab, choicesContainer);
            btn.GetComponentInChildren<TMP_Text>().text = choice.text;
            int index = choice.index; string choiceText = choice.text;
            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                StopTimer(); lastChosenText = choiceText;
                story.ChooseChoiceIndex(index);
                GameState.AddViewers(ViewersPerQuestion);
                GameState.AddSignatures(SignaturesPerAnswer);
                StartCoroutine(NextStep());
            });
        }
    }

    IEnumerator NextStep()
    {
        questionPanel.SetActive(false); choicePanel.SetActive(false);
        replyButton.gameObject.SetActive(false);
        if (!story.canContinue && story.currentChoices.Count == 0) { FinishQuiz(); yield break; }
        yield return new WaitForSeconds(betweenDelay);
        LoadStep();
    }

    // Lecture sécurisée d'une variable Ink — retourne "" si elle n'existe pas
    string ReadVar(string varName)
    {
        try { return story.variablesState[varName]?.ToString() ?? ""; }
        catch { Debug.LogWarning("[Quiz] Variable introuvable dans le ink : " + varName); return ""; }
    }

    int ReadVarInt(string varName)
    {
        try { return (int)story.variablesState[varName]; }
        catch { Debug.LogWarning("[Quiz] Variable int introuvable dans le ink : " + varName); return 0; }
    }

    void FinishQuiz()
    {
        if (quizFinished) return;
        quizFinished = true;

        StopTimer();

        if (story == null)
        {
            Debug.LogWarning("[StreamQuestionUI] story null dans FinishQuiz !");
            GameState.Reset();
            return;
        }

        // Lecture sécurisée — ne plante pas si une variable manque dans le ink
        GameState.quizScore      = ReadVarInt("score");
        GameState.question_1     = ReadVar("question_1");
        GameState.reponse_1      = ReadVar("reponse_1");
        GameState.explication_q1 = ReadVar("explication_q1");
        GameState.question_2     = ReadVar("question_2");
        GameState.reponse_2      = ReadVar("reponse_2");
        GameState.explication_q2 = ReadVar("explication_q2");
        GameState.question_3     = ReadVar("question_3");
        GameState.reponse_3      = ReadVar("reponse_3");
        GameState.explication_q3 = ReadVar("explication_q3");
        GameState.question_4     = ReadVar("question_4");
        GameState.reponse_4      = ReadVar("reponse_4");
        GameState.explication_q4 = ReadVar("explication_q4");
        GameState.question_5     = ReadVar("question_5");
        GameState.reponse_5      = ReadVar("reponse_5");
        GameState.explication_q5 = ReadVar("explication_q5");


        questionPanel.SetActive(false);
        choicePanel.SetActive(false);

        if (dataSender != null) dataSender.SendResults();
        else Debug.LogWarning("[StreamQuestionUI] QuizDataSender non assigné !");

        story = null;
        GameState.Reset();
        Debug.Log("QUIZ FINI — Va parler au NPC pour voir ton bilan !");
    }
}