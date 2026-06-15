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

    [Header("Scènes")]
    public string creditsSceneName = "Credits";

    void Awake()
    {
        GameState.dayManager = this;
    }

    void Start()
    {
        // Lancer le timer du jour 1 au démarrage
        DayAndNightCycle cycle = GetCycle();
        if (cycle != null) cycle.ResumeTimer();
    }

    public void OnDayEnded()
    {
        Debug.Log("[DayManager] Jour " + GameState.currentDay + " terminé → TP hutte");

        // Couper le quiz en cours
        if (streamQuestionUI != null)
            streamQuestionUI.ForceFinish();

        GameState.SaveDayResults();
        GameState.isInHut = true;
        GameState.hasSeenBilan = false;

        // Reset du timer — il est en pause interne, ne repartira que via ResumeTimer()
        DayAndNightCycle cycle = GetCycle();
        if (cycle != null) cycle.ResetCycle();

        // TP hutte
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

        // TP monde
        if (player != null && worldSpawnPoint != null)
            player.transform.position = worldSpawnPoint.position;

        // Relancer le timer maintenant que tout est prêt
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