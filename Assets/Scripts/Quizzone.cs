using UnityEngine;

public class QuizZone : MonoBehaviour
{
    [Header("Jour")]
    [Tooltip("Jour auquel appartient cette question (1, 2 ou 3). La zone ne se déclenche que ce jour-là.")]
    public int assignedDay = 1;

    [Header("Question associée")]
    [Tooltip("Nom du knot dans Quiz.ink (ex: j1_q1, j2_q4, j3_q2...)")]
    public string knotName;

    [Tooltip("Numéro global de la question dans GameState (1-17)")]
    public int globalIndex;

    [Header("Références")]
    public StreamQuestionUI streamQuestionUI;

    [Header("Visuel (optionnel)")]
    public GameObject zoneVisual;

    private bool done = false;

    void Start()
    {
        done = false;

        // Force l'activation du zoneVisual d'abord pour s'assurer qu'il est bien là
        if (zoneVisual != null)
            zoneVisual.SetActive(true);

        UpdateVisualForCurrentDay();

        Debug.Log("[QuizZone] " + gameObject.name
            + " | assignedDay=" + assignedDay
            + " | currentDay=" + GameState.currentDay
            + " | zoneVisual=" + (zoneVisual == null ? "NULL" : zoneVisual.name)
            + " | active=" + (zoneVisual != null ? zoneVisual.activeSelf.ToString() : "N/A"));
    }

    public void ResetZone()
    {
        done = false;
        gameObject.SetActive(true);

        // Force l'activation avant de filtrer par jour
        if (zoneVisual != null)
            zoneVisual.SetActive(true);

        UpdateVisualForCurrentDay();

        Debug.Log("[QuizZone] ResetZone " + gameObject.name
            + " | assignedDay=" + assignedDay
            + " | currentDay=" + GameState.currentDay
            + " | visible=" + (zoneVisual != null ? zoneVisual.activeSelf.ToString() : "N/A"));
    }

    void UpdateVisualForCurrentDay()
    {
        if (zoneVisual == null) return;
        bool isRightDay = GameState.currentDay == assignedDay;
        zoneVisual.SetActive(isRightDay && !done);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (done) return;

        if (GameState.currentDay != assignedDay)
        {
            Debug.Log("[QuizZone] Zone jour " + assignedDay + " ignorée (jour actuel = " + GameState.currentDay + ")");
            return;
        }

        if (streamQuestionUI == null) { Debug.LogWarning("[QuizZone] streamQuestionUI non assigné !"); return; }
        if (!GameState.CanStartQuestion()) return;

        done = true;
        if (zoneVisual != null) zoneVisual.SetActive(false);

        streamQuestionUI.TriggerQuestion(knotName, globalIndex, OnQuestionDone);
    }

    void OnQuestionDone()
    {
        Debug.Log("[QuizZone] Question " + globalIndex + " répondue, zone désactivée.");
    }
}