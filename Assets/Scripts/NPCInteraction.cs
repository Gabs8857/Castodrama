using UnityEngine;
using UnityEngine.InputSystem;
using Ink.Runtime;

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

    private bool dejaLance = false;

    // =========================================================================
    void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogError($"{name}: ❌ AUCUN COLLIDER2D TROUVÉ ! Ajoute un Box Collider 2D et coche Is Trigger.", this);
            return;
        }

        if (!col.isTrigger)
        {
            Debug.LogWarning($"{name}: ⚠️ Le Collider2D doit être en mode 'Is Trigger' !", this);
            col.isTrigger = true;
        }
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

        // Appel avec le Knot spécifié
        DialogueManager.Instance.StartDialogue(inkJSON, startKnot);

        // Optionnel : Détruit le trigger après utilisation
        Destroy(gameObject);
    }

    // =========================================================================
    bool Autorise()
    {
        if (string.IsNullOrWhiteSpace(gateVariable))
        {
            return true;
        }

        if (DialogueManager.Instance?.DialogueVariables?.variables == null)
        {
            Debug.LogError("[NPCInteraction] ❌ DialogueVariables ou variables est NULL !");
            return false;
        }

        var vars = DialogueManager.Instance.DialogueVariables.variables;
        if (!vars.TryGetValue(gateVariable, out var value))
        {
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
}