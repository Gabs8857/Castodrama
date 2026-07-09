using UnityEngine;
using UnityEngine.SceneManagement;

public class DayManager : MonoBehaviour
{
    [Header("Points de téléportation")]
    public Transform hutSpawnPoint;
    public Transform worldSpawnPoint;

    [Header("Objets")]
    public GameObject hutBlocker;
    public GameObject player;

    [Header("Références")]
    public StreamQuestionUI streamQuestionUI;
    public NPCInteraction npcInteraction;
    public DayAndNightCycle dayNightCycle;
    public GrassSpawner grassSpawner;
    public QuizDataSender dataSender;

    [Header("Scènes")]
    public string creditsSceneName = "Credits";

    void Awake()
    {
        GameState.dayManager = this;
    }

    void Start()
    {
        DayAndNightCycle cycle = GetCycle();
        if (cycle != null) cycle.ResumeTimer();
    }

    public void OnDayEnded()
    {
        Debug.Log("[DayManager] Jour " + GameState.currentDay + " terminé → TP hutte");

        // Plus de ForceFinish : les zones non visitées restent simplement sans réponse
        if (streamQuestionUI != null)
            streamQuestionUI.StartNewDay(); // ferme proprement si une question est en cours

        GameState.SaveDayResults();

        // Envoi des réponses du jour — AVANT ResetDayVars qui les remettrait à zéro
        if (dataSender != null)
            dataSender.SendDayResults(GameState.currentDay);
        else
            Debug.LogWarning("[DayManager] QuizDataSender non assigné !");

        GameState.isInHut = true;
        GameState.hasSeenBilan = false;

        DayAndNightCycle cycle = GetCycle();
        if (cycle != null) cycle.ResetCycle();

        if (player != null && hutSpawnPoint != null)
            player.transform.position = hutSpawnPoint.position;

        if (hutBlocker != null) hutBlocker.SetActive(true);

        if (npcInteraction != null)
            npcInteraction.SetDayDialogue(GameState.currentDay);
    }

    public void OnBilanDone()
    {
        GameState.hasSeenBilan = true;
        Debug.Log("[DayManager] Bilan vu — jour " + GameState.currentDay);

        if (GameState.currentDay >= 3)
        {
            Debug.Log("[DayManager] Fin du jeu → Crédits");
            SceneManager.LoadScene(creditsSceneName);
            return;
        }

        AdvanceToNextDay();
    }

    void AdvanceToNextDay()
    {
        GameState.currentDay++;
        GameState.ResetDayVars();
        GameState.isInHut = false;

        if (hutBlocker != null) hutBlocker.SetActive(false);

        Debug.Log("[DayManager] → Jour " + GameState.currentDay + " commence !");

        if (player != null && worldSpawnPoint != null)
            player.transform.position = worldSpawnPoint.position;

        // Réinitialiser toutes les zones de quiz — true pour inclure les zones désactivées
        Quizzone[] zones = FindObjectsOfType<Quizzone>(true);
        Debug.Log("[DayManager] Reset de " + zones.Length + " zones pour le jour " + GameState.currentDay);
        foreach (Quizzone zone in zones)
            zone.ResetZone();

        if (GameState.grassSpawner != null)
            GameState.grassSpawner.RespawnAll();
        else
            Debug.LogError("[DayManager] GameState.grassSpawner est NULL !");

        DayAndNightCycle cycle = GetCycle();
        if (cycle != null) cycle.ResumeTimer();

        if (streamQuestionUI != null)
            streamQuestionUI.StartNewDay();
    }

    DayAndNightCycle GetCycle()
    {
        return dayNightCycle != null ? dayNightCycle : FindObjectOfType<DayAndNightCycle>();
    }
}