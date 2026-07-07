using UnityEngine;
using UnityEngine.InputSystem;
using Ink.Runtime;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class NPCInteraction : MonoBehaviour
{
    [Header("⚙️ CONFIGURATION DU DIALOGUE")]
    [Tooltip("Fichier Ink contenant TOUS les Knots (ex: tuto.ink)")]
    [SerializeField] private TextAsset inkJSON;

    [Tooltip("Nom du Knot à lancer (ex: tutodebut, tutomarche, tutocarte)")]
    [SerializeField] private string startKnot = "";

    [Tooltip("Tag du joueur (doit correspondre au tag de ton personnage)")]
    [SerializeField] private string tagDetecte = "Player";

    [Tooltip("Variable Ink requise pour activer ce trigger (ex: tutomarche_done)")]
    [SerializeField] private string gateVariable = "";

    private Collider2D interactionCollider;
    private bool dejaLance = false;

    // =========================================================================
    void Awake()
    {
        interactionCollider = GetComponent<Collider2D>();
        if (interactionCollider == null)
        {
            Debug.LogError($"{name}: ❌ AUCUN COLLIDER2D TROUVÉ ! Ajoute un Box Collider 2D et coche Is Trigger.", this);
            return;
        }

        if (!interactionCollider.isTrigger)
        {
            Debug.LogWarning($"{name}: ⚠️ Le Collider2D doit être en mode 'Is Trigger' !", this);
            interactionCollider.isTrigger = true;
        }

        UpdateTriggerState();
    }

    // =========================================================================
    void Update()
    {
        UpdateTriggerState();
    }

    // =========================================================================
    void OnTriggerEnter2D(Collider2D other)
    {
        // Vérification du tag
        if (dejaLance || !other.CompareTag(tagDetecte))
        {
            return;
        }

        // Vérification du fichier Ink
        if (inkJSON == null)
        {
            Debug.LogError($"{name}: ❌ Ink JSON non assigné !", this);
            return;
        }

        // Vérification de la condition (gateVariable)
        if (!string.IsNullOrEmpty(gateVariable) && !Autorise())
        {
            return;
        }

        // Vérification de DialogueManager
        if (DialogueManager.Instance == null)
        {
            Debug.LogError("[NPCInteraction] ❌ DialogueManager.Instance est NULL !");
            return;
        }

        // ✅ TOUT EST OK, LANCEMENT DU DIALOGUE
        Debug.Log($"[NPCInteraction] {name}: ✅ Lancement du dialogue '{inkJSON.name}', Knot: '{startKnot}'");
        dejaLance = true;

        if (GameState.Mode != GameMode.Free)
        {
            Debug.Log($"[NPCInteraction] {name}: GameState occupé ({GameState.Mode}) → fermeture puis lancement différé.");

            if (DialogueManager.Instance.IsDialogueOpen)
            {
                DialogueManager.Instance.FinishDialogueNow();
            }

            StartCoroutine(LaunchDialogueNextFrame(inkJSON, startKnot));
        }
        else
        {
            // Appel avec le Knot spécifié
            DialogueManager.Instance.StartDialogue(inkJSON, startKnot);
        }

        if (TutorialManager.Instance != null)
            TutorialManager.Instance.OnPlayerInteracted();

        // Optionnel : Détruit le trigger après utilisation
        Destroy(gameObject);
    }

    // =========================================================================
    bool Autorise()
    {
        return Autorise(true);
    }

    // =========================================================================
    bool Autorise(bool logErrors)
    {
        if (string.IsNullOrWhiteSpace(gateVariable))
        {
            return true;
        }

        if (DialogueManager.Instance?.DialogueVariables?.variables == null)
        {
            if (logErrors)
                Debug.LogError("[NPCInteraction] ❌ DialogueVariables ou variables est NULL !");
            return false;
        }

        var vars = DialogueManager.Instance.DialogueVariables.variables;
        if (!vars.TryGetValue(gateVariable, out var value))
        {
            if (logErrors)
                Debug.LogError($"[NPCInteraction] ❌ Variable '{gateVariable}' introuvable !");
            return false;
        }

        // Gestion des types Ink
        if (value is BoolValue boolValue) return boolValue.value;
        if (value is IntValue intValue) return intValue.value != 0;

        Debug.LogWarning($"[NPCInteraction] ⚠️ Type de variable '{gateVariable}' non supporté: {value.GetType().Name}");
        return false;
    }

    // =========================================================================
    // Pour compatibilité avec DayManager
    public void SetDayDialogue(int day) { }

    // =========================================================================
    void UpdateTriggerState()
    {
        if (interactionCollider == null)
        {
            return;
        }

        bool triggerAutorise = string.IsNullOrWhiteSpace(gateVariable) || Autorise(false);
        if (interactionCollider.enabled != triggerAutorise)
        {
            interactionCollider.enabled = triggerAutorise;
        }
    }

    private IEnumerator LaunchDialogueNextFrame(TextAsset dialogAsset, string knotName)
    {
        yield return null;

        if (DialogueManager.Instance == null)
        {
            Debug.LogError($"[NPCInteraction] {name}: DialogueManager.Instance est NULL au lancement différé !");
            yield break;
        }

        if (GameState.Mode != GameMode.Free)
            GameState.Reset();

        DialogueManager.Instance.StartDialogue(dialogAsset, knotName);
    }
}