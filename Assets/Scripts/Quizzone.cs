using UnityEngine;

// QuizZone — à poser sur chaque collider trigger de la map.
// Configure dans l'Inspector :
//   - knotName      : nom du knot ink à jouer (ex: "j1_q1", "j2_q3"...)
//   - globalIndex   : numéro global de la question (1-17)
//   - streamQuestionUI : référence au StreamQuestionUI de la scène

public class Quizzone : MonoBehaviour
{
    [Header("Question associée")]
    [Tooltip("Nom du knot dans Quiz.ink (ex: j1_q1, j2_q4, j3_q2...)")]
    public string knotName;

    [Tooltip("Numéro global de la question dans GameState (1-17)")]
    public int globalIndex;

    [Header("Références")]
    public StreamQuestionUI streamQuestionUI;

    // Rendu optionnel : objet visuel à désactiver quand la zone est épuisée
    [Header("Visuel (optionnel)")]
    public GameObject zoneVisual;

    private bool done = false;

    void Start()
    {
        // Ré-activer la zone au démarrage d'un nouveau jour si besoin
        done = false;
        if (zoneVisual != null) zoneVisual.SetActive(true);
    }

    // Appelé par DayManager au début d'un nouveau jour
    public void ResetZone()
    {
        done = false;
        gameObject.SetActive(true);
        if (zoneVisual != null) zoneVisual.SetActive(true);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (done) return;
        if (streamQuestionUI == null) { Debug.LogWarning("[QuizZone] streamQuestionUI non assigné !"); return; }
        if (!GameState.CanStartQuestion()) return;

        done = true;
        if (zoneVisual != null) zoneVisual.SetActive(false);

        streamQuestionUI.TriggerQuestion(knotName, globalIndex, OnQuestionDone);

        // Hook tuto
        if (TutorialManager.Instance != null)
            TutorialManager.Instance.OnQuizTriggered();
    }

    void OnQuestionDone()
    {
        Debug.Log("[QuizZone] Question " + globalIndex + " répondue, zone désactivée.");
        // La zone reste done = true jusqu'au prochain ResetZone (nouveau jour)
    }
}