using UnityEngine;

/// <summary>
/// Gère le ramassage de boue en nage profonde.
/// Appuie sur G sous l'eau → joue l'animation de création, puis spawne un MudCloud
/// qui file vers le barrage. Pas de limite de boue, pas d'icône UI.
/// </summary>
public class MudSystem : MonoBehaviour
{
    [Header("Mud Cloud")]
    [Tooltip("Prefab du nuage de boue, avec SpriteResolver + SpriteLibrary configurée (catégories MudMove et MudImpact)")]
    [SerializeField] private GameObject mudCloudPrefab;
    [SerializeField] private float cloudSpeed = 2f;
    [Tooltip("Sprite statique de secours, utilisé uniquement si mudCloudPrefab est vide")]
    [SerializeField] private Sprite mudCloudSprite;

    [Header("Cooldown")]
    [SerializeField] private float mudCooldown = 1f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private CharacterAnimator characterAnimator;
    private MudCreationAnimator mudCreationAnimator;
    private DamManager damManager;
    private float lastMudTime = -10f;

    public bool HasMud => false; // gardé pour compatibilité

    private void Awake()
    {
        characterAnimator = GetComponent<CharacterAnimator>();
        mudCreationAnimator = GetComponent<MudCreationAnimator>();

        if (mudCreationAnimator == null)
            Debug.LogWarning("[MudSystem] ⚠ MudCreationAnimator introuvable — la boue sera créée sans animation dédiée.");

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

        // Si une animation de création est en cours, on ignore le nouvel appui
        if (mudCreationAnimator != null && mudCreationAnimator.IsPlaying)
        {
            if (debugLogs)
                Debug.Log("[MudSystem] Appui ignoré — animation de création déjà en cours.");
            return;
        }

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

        CreateMud();
        lastMudTime = Time.time;
    }

    private void CreateMud()
    {
        // Joue l'animation de création (bloque le mouvement pendant la durée de l'anim)
        if (mudCreationAnimator != null)
            mudCreationAnimator.PlayMudCreation();

        SpawnMudCloud();
    }

    private void SpawnMudCloud()
    {
        MudCloud cloud;

        if (mudCloudPrefab != null)
        {
            cloud = MudCloud.SpawnFromPrefab(mudCloudPrefab, transform.position, damManager);
        }
        else
        {
            cloud = MudCloud.Spawn(transform.position, damManager, mudCloudSprite);
            if (debugLogs)
                Debug.LogWarning("[MudSystem] ⚠ Aucun prefab assigné — nuage sans animation de frames créé.");
        }

        if (debugLogs)
            Debug.Log($"[MudSystem] ☁ Nuage de boue créé en {transform.position}");
    }

    public bool UseMud() => false; // gardé pour compatibilité
}
