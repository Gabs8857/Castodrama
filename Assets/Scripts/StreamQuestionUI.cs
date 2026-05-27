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

    // Garde en mémoire le texte du choix qu'on vient de faire pour l'ignorer
    private string lastChosenText = "";

    void Start()
    {
        questionPanel.SetActive(false);
        choicePanel.SetActive(false);

        replyButton.onClick.AddListener(OpenChoices);

        StartCoroutine(StartQuiz());
    }

    IEnumerator StartQuiz()
    {
        if (!GameState.CanStartQuestion())
            yield break;

        GameState.Set(GameMode.Question);

        story = new Story(inkJSON.text);

        yield return new WaitForSeconds(startDelay);

        LoadStep();
    }

    void LoadStep()
    {
        string currentText = "";

        if (story == null) return;

        if (!story.canContinue && story.currentChoices.Count == 0)
        {
            FinishQuiz();
            return;
        }

        while (story.canContinue)
        {
            string line = story.Continue();

            // Ignorer la ligne si c'est le texte du choix qu'on vient de sélectionner
            if (!string.IsNullOrEmpty(line) && line.Trim() != lastChosenText.Trim())
                currentText += line + "\n";

            if (story.currentChoices.Count > 0)
                break;
        }

        // Reset pour la prochaine étape
        lastChosenText = "";

        // Rien de valide à afficher + pas de choix = fin
        if (string.IsNullOrEmpty(currentText.Trim()) && story.currentChoices.Count == 0)
        {
            FinishQuiz();
            return;
        }

        questionText.text = currentText;

        questionPanel.SetActive(true);
        choicePanel.SetActive(false);
        replyButton.gameObject.SetActive(true);
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
        foreach (Transform c in choicesContainer)
            Destroy(c.gameObject);

        foreach (Choice choice in story.currentChoices)
        {
            GameObject btn = Instantiate(choicePrefab, choicesContainer);
            btn.GetComponentInChildren<TMP_Text>().text = choice.text;

            int index = choice.index;
            string choiceText = choice.text;

            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                lastChosenText = choiceText; // mémorise ce qu'on a cliqué
                story.ChooseChoiceIndex(index);
                StartCoroutine(NextStep());
            });
        }
    }

    IEnumerator NextStep()
    {
        questionPanel.SetActive(false);
        choicePanel.SetActive(false);
        replyButton.gameObject.SetActive(false);

        if (!story.canContinue && story.currentChoices.Count == 0)
        {
            FinishQuiz();
            yield break;
        }

        yield return new WaitForSeconds(betweenDelay);

        LoadStep();
    }

    void FinishQuiz()
    {
        GameState.quizScore = (int)story.variablesState["score"];
        GameState.firstAnswer = story.variablesState["firstAnswer"].ToString();
        GameState.secondAnswer = story.variablesState["secondAnswer"].ToString();
        GameState.q1Explanation = story.variablesState["q1_explanation"].ToString();
        GameState.q2Explanation = story.variablesState["q2_explanation"].ToString();

        questionPanel.SetActive(false);
        choicePanel.SetActive(false);

        GameState.Reset();

        Debug.Log("QUIZ FINI — Va parler au NPC pour voir ton bilan !");
    }
}