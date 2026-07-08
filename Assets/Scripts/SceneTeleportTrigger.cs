using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTeleportTrigger : MonoBehaviour
{
    [Header("Scène cible")]
    public string sceneName = "Rivière";

    [Header("Options")]
    public string playerTag = "Player";
    public string requiredVar = "fintuto"; // variable Ink à vérifier

    [Header("Visuel (optionnel)")]
    public GameObject visualIndicator; // icône/flèche à afficher quand actif

    private Collider2D col;
    private bool hasTriggered = false;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        col.enabled = false; // désactivé tant que le tuto n'est pas fini

        if (visualIndicator != null)
            visualIndicator.SetActive(false);
    }

    void Update()
    {
        if (col.enabled) return; // déjà actif, plus besoin de vérifier

        if (DialogueManager.Instance != null &&
            DialogueManager.Instance.GetGlobalBool(requiredVar))
        {
            ActivateTrigger();
        }
    }

    void ActivateTrigger()
    {
        col.enabled = true;

        if (visualIndicator != null)
            visualIndicator.SetActive(true);

        Debug.Log($"[SceneTeleportTrigger] '{requiredVar}' = true → trigger activé.");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag(playerTag)) return;

        hasTriggered = true;
        SceneManager.LoadScene(sceneName);
    }
}