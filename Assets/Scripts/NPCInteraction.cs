using UnityEngine;
using UnityEngine.InputSystem;
using Ink.Runtime;

[RequireComponent(typeof(Collider2D))] // ← CHANGÉ POUR COLLIDER2D
public class NPCInteraction : MonoBehaviour
{
    [Header("Ink à lancer")]
    [SerializeField] private TextAsset inkJSON;

    [Header("Tag détecté")]
    [SerializeField] private string tagDetecte = "Player";

    [Header("Variable requise")]
    [SerializeField] private string gateVariable = "";

    private bool dejaLance = false;

    void Awake()
    {
        Collider2D col = GetComponent<Collider2D>(); // ← CHANGÉ POUR COLLIDER2D
        if (col == null)
        {
            Debug.LogError($"{name}: AUCUN COLLIDER2D TROUVÉ ! Ajoute un Box Collider 2D et coche Is Trigger.", this);
            return;
        }
        if (!col.isTrigger)
        {
            Debug.LogWarning($"{name}: Le Collider2D doit être en mode 'Is Trigger' !", this);
            col.isTrigger = true;
        }
    }

    // ← CHANGÉ POUR OnTriggerEnter2D (2D)
    void OnTriggerEnter2D(Collider2D other)
    {
        if (dejaLance || !other.CompareTag(tagDetecte)) return;

        if (inkJSON == null)
        {
            Debug.LogError($"{name}: Ink JSON non assigné !", this);
            return;
        }

        if (!string.IsNullOrEmpty(gateVariable) && !Autorise()) return;

        if (DialogueManager.Instance == null)
        {
            Debug.LogError("[NPCInteraction] DialogueManager.Instance est NULL !");
            return;
        }

        Debug.Log($"[NPCInteraction] ✅ Lancement du dialogue: {inkJSON.name}");
        dejaLance = true;
        DialogueManager.Instance.StartDialogue(inkJSON);
        Destroy(gameObject);
    }

    bool Autorise()
    {
        if (string.IsNullOrWhiteSpace(gateVariable)) return true;

        var vars = DialogueManager.Instance?.DialogueVariables?.variables;
        if (vars == null) return false;

        if (vars.TryGetValue(gateVariable, out var value))
        {
            if (value is BoolValue boolValue) return boolValue.value;
            if (value is IntValue intValue) return intValue.value != 0;
        }
        return false;
    }

    public void SetDayDialogue(int day) { }
}