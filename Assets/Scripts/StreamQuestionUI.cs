using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using Ink.Runtime;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Gère l'affichage des questions de quiz (via Ink) et la validation des réponses.
/// Supporte la souris (clic sur les Toggle) et la manette (stick droit + A via GamepadToggleNavigator).
/// </summary>
public class StreamQuestionUI : MonoBehaviour
{
    [Header("Ink — Quiz (fichier unique)")]
    [SerializeField] private TextAsset quizInk;

    [Header("UI")]
    [SerializeField] private GameObject questionPanel;
    [SerializeField] private GameObject choicePanel;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text choiceQuestionText;
    [SerializeField] private Button replyButton;
    [SerializeField] private Button validateButton;

    [Header("Choices")]
    [SerializeField] private Transform choicesContainer;
    [SerializeField] private GameObject togglePrefab;

    [Header("Gamepad")]
    [Tooltip("Navigation manette (stick droit + A) sur les propositions")]
    [SerializeField] private GamepadToggleNavigator gamepadNavigator;

    [Header("BDD")]
    [SerializeField] private QuizDataSender dataSender;

    private const int ViewersPerQuestion  = 150;
    private const int SignaturesPerAnswer = 100;

    // Index relatif → variables GameState par jour
    private static readonly int[] DayOffsets = { 0, 0, 5, 11 }; // index 0 inutilisé

    private class ChoiceData
    {
        public string text;
        public bool isCorrect;
        public Toggle toggle;
    }

    private Story story;
    private readonly List<ChoiceData> currentChoices = new List<ChoiceData>();
    private int currentGlobalIndex = 0;
    private System.Action onDone;

    private void Start()
    {
        questionPanel.SetActive(false);
        choicePanel.SetActive(false);
        replyButton.onClick.AddListener(OpenChoices);
        validateButton.onClick.AddListener(ValidateChoices);
        validateButton.gameObject.SetActive(false);
    }

    private void Update()
    {
        HandleGamepadButtons();
    }

    /// <summary>
    /// Permet d'appuyer sur A pour cliquer le bouton Répondre sur l'écran de question.
    /// Le bouton Valider, lui, est géré par GamepadToggleNavigator (intégré à la
    /// navigation des toggles) pour éviter qu'un A destiné à cocher une case
    /// ne déclenche Valider par accident.
    /// </summary>
    private void HandleGamepadButtons()
    {
        if (Gamepad.current == null) return;
        if (!Gamepad.current.buttonSouth.wasPressedThisFrame) return;

        if (questionPanel.activeSelf && replyButton.gameObject.activeSelf && replyButton.interactable)
        {
            replyButton.onClick.Invoke();
        }
    }

    // ── API publique ─────────────────────────────────────────────────────

    public void TriggerQuestion(string knotName, int globalIndex, System.Action onFinished)
    {
        if (!GameState.CanStartQuestion())
        {
            Debug.Log("[Quiz] Impossible de lancer, mode = " + GameState.Mode);
            return;
        }

        if (quizInk == null)
        {
            Debug.LogWarning("[Quiz] quizInk non assigné !");
            return;
        }

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

    // ── Affichage ────────────────────────────────────────────────────────

    private void ShowQuestion()
    {
        if (story == null) return;

        string currentText = ContinueStoryUntilChoice();

        if (string.IsNullOrWhiteSpace(currentText) && story.currentChoices.Count == 0)
        {
            Finish();
            return;
        }

        questionText.text = currentText.Trim();
        questionPanel.SetActive(true);
        choicePanel.SetActive(false);
        replyButton.gameObject.SetActive(true);
        validateButton.gameObject.SetActive(false);
    }

    private void OpenChoices()
    {
        questionPanel.SetActive(false);
        replyButton.gameObject.SetActive(false);

        if (choiceQuestionText != null)
            choiceQuestionText.text = questionText.text;

        choicePanel.SetActive(true);
        validateButton.gameObject.SetActive(true);
        BuildToggles();
    }

    private void BuildToggles()
    {
        ClearContainer();
        currentChoices.Clear();

        foreach (Choice choice in story.currentChoices)
        {
            bool isCorrect = false;
            foreach (string tag in choice.tags ?? new List<string>())
            {
                if (tag.Trim() == "correct")
                {
                    isCorrect = true;
                    break;
                }
            }

            GameObject obj = Instantiate(togglePrefab, choicesContainer);
            Toggle toggle = obj.GetComponent<Toggle>();
            TMP_Text label = obj.GetComponentInChildren<TMP_Text>(true);

            if (label != null) label.text = choice.text;
            if (toggle != null) toggle.isOn = false;

            currentChoices.Add(new ChoiceData { text = choice.text, isCorrect = isCorrect, toggle = toggle });
        }

        // Branche la navigation manette sur les toggles nouvellement créés
        if (gamepadNavigator != null)
            gamepadNavigator.SetContainer(choicesContainer);
    }

    private void ClearContainer()
    {
        List<GameObject> toDestroy = new List<GameObject>();
        foreach (Transform c in choicesContainer)
            if (c != null) toDestroy.Add(c.gameObject);

        foreach (GameObject go in toDestroy)
            if (go != null) Destroy(go);
    }

    // ── Validation ───────────────────────────────────────────────────────

    private void ValidateChoices()
    {
        ChoiceData chosen = GetCheckedChoiceOrFallback();
        if (chosen == null) return;

        List<string> checkedTexts = GetCheckedTexts(chosen);
        List<string> correctTexts = GetCorrectTexts();

        int choiceIndex = FindStoryChoiceIndex(chosen.text);
        story.ChooseChoiceIndex(choiceIndex);

        choicePanel.SetActive(false);
        validateButton.gameObject.SetActive(false);

        string nextText = ContinueStoryUntilChoice();

        // S'il y a d'autres choix (ex: enchaînement Drama) → nouvel écran
        if (story.currentChoices.Count > 0)
        {
            questionText.text = nextText.Trim();
            questionPanel.SetActive(true);
            choicePanel.SetActive(false);
            replyButton.gameObject.SetActive(true);
            validateButton.gameObject.SetActive(false);
            return;
        }

        // Fin de la question — sauvegarde du dernier écran validé
        bool allCorrectChecked = correctTexts.Count > 0
            && checkedTexts.TrueForAll(t => correctTexts.Contains(t))
            && correctTexts.TrueForAll(t => checkedTexts.Contains(t));

        GameState.AddViewers(ViewersPerQuestion);
        GameState.AddSignatures(SignaturesPerAnswer);

        SaveAnswerToGameState(string.Join(", ", checkedTexts), allCorrectChecked, correctTexts);
        Finish();
    }

    private ChoiceData GetCheckedChoiceOrFallback()
    {
        foreach (ChoiceData cd in currentChoices)
            if (cd.toggle != null && cd.toggle.isOn)
                return cd;

        // Aucun choix coché → fallback sur le premier (évite de bloquer le joueur)
        return currentChoices.Count > 0 ? currentChoices[0] : null;
    }

    private List<string> GetCheckedTexts(ChoiceData fallbackChoice)
    {
        List<string> checkedTexts = new List<string>();
        foreach (ChoiceData cd in currentChoices)
            if (cd.toggle != null && cd.toggle.isOn)
                checkedTexts.Add(cd.text);

        if (checkedTexts.Count == 0)
            checkedTexts.Add(fallbackChoice.text);

        return checkedTexts;
    }

    private List<string> GetCorrectTexts()
    {
        List<string> correctTexts = new List<string>();
        foreach (ChoiceData cd in currentChoices)
            if (cd.isCorrect)
                correctTexts.Add(cd.text);
        return correctTexts;
    }

    private int FindStoryChoiceIndex(string choiceText)
    {
        for (int i = 0; i < story.currentChoices.Count; i++)
            if (story.currentChoices[i].text == choiceText)
                return i;
        return 0;
    }

    /// <summary>
    /// Avance l'histoire Ink jusqu'au prochain point de choix, en concaténant le texte affiché.
    /// </summary>
    private string ContinueStoryUntilChoice()
    {
        string text = "";
        while (story.canContinue)
        {
            string line = story.Continue();
            if (!string.IsNullOrWhiteSpace(line))
                text += line.Trim() + "\n";
            if (story.currentChoices.Count > 0) break;
        }
        return text;
    }

    // ── Sauvegarde ───────────────────────────────────────────────────────

    /// <summary>
    /// Sauvegarde dans les variables relatives (reponse_1 à reponse_6) en convertissant
    /// le globalIndex en index relatif au jour courant.
    /// Jour 1 : globalIndex 1-5  → relatif 1-5
    /// Jour 2 : globalIndex 6-11 → relatif 1-6
    /// Jour 3 : globalIndex 12-17 → relatif 1-6
    /// </summary>
    private void SaveAnswerToGameState(string reponsesStr, bool isCorrect, List<string> correctTexts)
    {
        string explication = isCorrect
            ? "Bonne reponse !"
            : "Mauvaise reponse. La bonne etait : " + string.Join(", ", correctTexts);

        if (isCorrect) GameState.quizScore++;

        int dayOffset = (GameState.currentDay >= 1 && GameState.currentDay <= 3)
            ? DayOffsets[GameState.currentDay]
            : 0;
        int relativeIndex = currentGlobalIndex - dayOffset;

        Debug.Log($"[Quiz] SaveAnswer globalIndex={currentGlobalIndex} jour={GameState.currentDay} "
            + $"relativeIndex={relativeIndex} reponse={reponsesStr}");

        switch (relativeIndex)
        {
            case 1: GameState.reponse_1 = reponsesStr; GameState.explication_q1 = explication; break;
            case 2: GameState.reponse_2 = reponsesStr; GameState.explication_q2 = explication; break;
            case 3: GameState.reponse_3 = reponsesStr; GameState.explication_q3 = explication; break;
            case 4: GameState.reponse_4 = reponsesStr; GameState.explication_q4 = explication; break;
            case 5: GameState.reponse_5 = reponsesStr; GameState.explication_q5 = explication; break;
            case 6: GameState.reponse_6 = reponsesStr; GameState.explication_q6 = explication; break;
            default:
                Debug.LogWarning($"[Quiz] relativeIndex hors range : {relativeIndex} "
                    + $"(globalIndex={currentGlobalIndex}, jour={GameState.currentDay})");
                break;
        }
    }

    private void Finish()
    {
        questionPanel.SetActive(false);
        choicePanel.SetActive(false);
        story = null;
        GameState.Reset();

        Debug.Log("[Quiz] Question " + currentGlobalIndex + " terminée.");

        if (dataSender != null)
            dataSender.SendDayResults(GameState.currentDay);

        onDone?.Invoke();
        onDone = null;
    }
}