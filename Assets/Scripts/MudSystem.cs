using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Gère le ramassage de boue en nage profonde.
/// Appuie sur G sous l'eau → spawne un MudCloud qui file vers le barrage.
/// Pas de limite de boue, pas d'icône UI — le nuage gère tout.
/// </summary>
public class MudSystem : MonoBehaviour
{
    [Header("Mud Cloud")]
    [Tooltip("Vitesse du nuage de boue vers le barrage")]
    [SerializeField] private float cloudSpeed = 2f;

    [Header("Cooldown")]
    [SerializeField] private float mudCooldown = 1f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private CharacterAnimator characterAnimator;
    private DamManager damManager;
    private float lastMudTime = -10f;

    // Gardé pour compatibilité avec DamManager (CheckMudRepair supprimé donc inutile,
    // mais on le laisse au cas où d'autres scripts y accèdent)
    public bool HasMud => false;

    private void Awake()
    {
        characterAnimator = GetComponent<CharacterAnimator>();
        Debug.Log("[MudSystem] ✓ Système de boue initialisé.");
    }

    private void Start()
    {
        damManager = FindObjectOfType<DamManager>();
        if (damManager == null)
            Debug.LogWarning("[MudSystem] ✗ DamManager introuvable !");
        else if (debugLogs)
            Debug.Log("[MudSystem] ✓ DamManager lié.");
    }

    private void Update()
    {
        if (!InputHelper.GrabPressed()) return;

        if (!characterAnimator.IsSwimmingDeep)
        {
            if (debugLogs)
                Debug.Log("[MudSystem] Impossible — pas en nage profonde.");
            return;
        }

        if (Time.time - lastMudTime < mudCooldown)
        {
            if (debugLogs)
                Debug.Log($"[MudSystem] Cooldown ({mudCooldown - (Time.time - lastMudTime):F1}s restantes)");
            return;
        }

        if (damManager == null)
        {
            Debug.LogWarning("[MudSystem] DamManager introuvable, impossible de spawner un nuage.");
            return;
        }

        SpawnMudCloud();
        lastMudTime = Time.time;
    }

    private void SpawnMudCloud()
    {
        MudCloud cloud = MudCloud.Spawn(transform.position, damManager);
        if (debugLogs)
            Debug.Log($"[MudSystem] ☁ Nuage de boue créé en {transform.position}");
    }

    // Gardé pour compatibilité — ne fait plus rien
    public bool UseMud() => false;
}
