using UnityEngine;
using UnityEngine.U2D.Animation;

/// <summary>
/// Joue une animation courte (une fois) quand le joueur crée un nuage de boue (touche G).
/// Bloque le mouvement pendant la durée de l'animation, puis redonne le contrôle automatiquement.
/// 
/// À appeler depuis MudSystem au moment où le nuage est spawné :
///   mudCreationAnimator.PlayMudCreation();
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class MudCreationAnimator : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private string mudCreationCategoryName = "MudCreate";
    [SerializeField] private string[] mudCreationFrameNames = { "Frame1", "Frame2", "Frame3" };
    [SerializeField] private float frameSwitchSpeed = 0.1f;
    [Tooltip("Durée minimale totale de l'animation, même si toutes les frames sont jouées plus vite. Le cycle de frames se répète jusqu'à atteindre cette durée.")]
    [SerializeField] private float minimumDuration = 0.6f;

    [Header("Comportement")]
    [Tooltip("Bloque le mouvement du joueur pendant l'animation")]
    [SerializeField] private bool blockMovementDuringAnim = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private SpriteResolver spriteResolver;
    private Rigidbody2D rb;
    private PlayerController playerController;
    private CharacterAnimator characterAnimator;

    private bool isPlaying = false;
    private float timeSinceLastSwitch = 0f;
    private int currentFrameIndex = 0;
    private Vector2 lockedPosition;
    private float elapsedPlayTime = 0f;

    public bool IsPlaying => isPlaying;

    private void Awake()
    {
        spriteResolver = GetComponent<SpriteResolver>();
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
        characterAnimator = GetComponent<CharacterAnimator>();

        Debug.Log("═══════════════════════════════════════");
        Debug.Log($"[MudCreationAnimator] INIT sur '{gameObject.name}'");
        Debug.Log($"[MudCreationAnimator] SpriteResolver: {(spriteResolver != null ? "✓ TROUVÉ" : "❌ MANQUANT")}");
        Debug.Log($"[MudCreationAnimator] Rigidbody2D: {(rb != null ? "✓ TROUVÉ" : "❌ MANQUANT")}");
        Debug.Log($"[MudCreationAnimator] PlayerController: {(playerController != null ? "✓ TROUVÉ" : "❌ MANQUANT")}");
        Debug.Log($"[MudCreationAnimator] Catégorie configurée: '{mudCreationCategoryName}'");
        Debug.Log($"[MudCreationAnimator] Frames configurées: [{string.Join(", ", mudCreationFrameNames)}]");

        if (spriteResolver == null)
            Debug.LogError($"[MudCreationAnimator] ❌ CRITIQUE: Pas de SpriteResolver sur {gameObject.name} !");

        if (spriteResolver != null && mudCreationFrameNames.Length > 0)
            TestCategoryExists();

        Debug.Log("═══════════════════════════════════════");
    }

    private void TestCategoryExists()
    {
        try
        {
            string currentCategory = spriteResolver.GetCategory();
            string currentLabel = spriteResolver.GetLabel();

            spriteResolver.SetCategoryAndLabel(mudCreationCategoryName, mudCreationFrameNames[0]);
            string appliedCategory = spriteResolver.GetCategory();
            string appliedLabel = spriteResolver.GetLabel();

            bool success = appliedCategory == mudCreationCategoryName && appliedLabel == mudCreationFrameNames[0];

            if (success)
                Debug.Log($"[MudCreationAnimator] ✓ Catégorie '{mudCreationCategoryName}' / Label '{mudCreationFrameNames[0]}' EXISTE.");
            else
                Debug.LogError($"[MudCreationAnimator] ❌ Catégorie '{mudCreationCategoryName}' / Label '{mudCreationFrameNames[0]}' INTROUVABLE dans la Sprite Library ! "
                    + $"Obtenu : catégorie='{appliedCategory}', label='{appliedLabel}'. Vérifie l'orthographe exacte (sensible à la casse).");

            if (!string.IsNullOrEmpty(currentCategory))
                spriteResolver.SetCategoryAndLabel(currentCategory, currentLabel);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[MudCreationAnimator] ❌ Exception lors du test de catégorie : {e.Message}");
        }
    }

    private void Update()
    {
        if (!isPlaying) return;

        elapsedPlayTime += Time.deltaTime;

        if (debugLogs && Time.frameCount % 10 == 0)
            Debug.Log($"[MudCreationAnimator] En cours — frame {currentFrameIndex}/{mudCreationFrameNames.Length}, temps écoulé {elapsedPlayTime:F2}s/{minimumDuration:F2}s");

        if (blockMovementDuringAnim && rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            if (rb.position != lockedPosition)
                rb.position = lockedPosition;
        }

        timeSinceLastSwitch += Time.deltaTime;
        if (timeSinceLastSwitch >= frameSwitchSpeed)
        {
            timeSinceLastSwitch = 0f;
            AdvanceFrame();
        }
    }

    private void FixedUpdate()
    {
        if (isPlaying && blockMovementDuringAnim && rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.position = lockedPosition;
        }
    }

    private void AdvanceFrame()
    {
        SwitchFrame();
        currentFrameIndex++;

        // Boucle le cycle de frames tant que la durée minimale n'est pas atteinte
        if (currentFrameIndex >= mudCreationFrameNames.Length)
        {
            if (elapsedPlayTime >= minimumDuration)
            {
                EndAnimation();
            }
            else
            {
                currentFrameIndex = 0; // recommence le cycle
                if (debugLogs)
                    Debug.Log($"[MudCreationAnimator] Cycle de frames terminé mais durée minimale pas atteinte ({elapsedPlayTime:F2}s/{minimumDuration:F2}s) — on boucle.");
            }
        }
    }

    private void SwitchFrame()
    {
        if (currentFrameIndex >= mudCreationFrameNames.Length) return;

        string label = mudCreationFrameNames[currentFrameIndex];
        try
        {
            spriteResolver.SetCategoryAndLabel(mudCreationCategoryName, label);
            if (debugLogs)
                Debug.Log($"[MudCreationAnimator] 🎬 Frame '{label}' (index {currentFrameIndex})");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[MudCreationAnimator] ❌ Erreur SetCategoryAndLabel: {e.Message}");
        }
    }

    /// <summary>
    /// Joue l'animation de création de boue une seule fois.
    /// Bloque le mouvement pendant la durée, puis redonne le contrôle automatiquement.
    /// </summary>
    public void PlayMudCreation()
    {
        Debug.Log($"[MudCreationAnimator] >>> PlayMudCreation() appelé. isPlaying actuel = {isPlaying}");

        if (isPlaying)
        {
            Debug.LogWarning("[MudCreationAnimator] PlayMudCreation() ignoré — animation déjà en cours.");
            return;
        }

        if (spriteResolver == null)
        {
            Debug.LogError("[MudCreationAnimator] ❌ Impossible de jouer l'animation — spriteResolver NULL !");
            return;
        }

        if (mudCreationFrameNames.Length == 0)
        {
            Debug.LogError("[MudCreationAnimator] ❌ Aucune frame configurée !");
            return;
        }

        isPlaying = true;
        currentFrameIndex = 0;
        timeSinceLastSwitch = 0f;
        elapsedPlayTime = 0f;

        if (blockMovementDuringAnim)
        {
            if (rb != null)
            {
                lockedPosition = rb.position;
                rb.linearVelocity = Vector2.zero;
            }

            if (playerController != null)
                playerController.enabled = false;
        }

        SwitchFrame();

        Debug.Log($"[MudCreationAnimator] 🟤 Animation de création de boue démarrée ({mudCreationFrameNames.Length} frames, blocage mouvement: {blockMovementDuringAnim})");
    }

    private void EndAnimation()
    {
        isPlaying = false;

        if (blockMovementDuringAnim && playerController != null)
            playerController.enabled = true;

        Debug.Log("[MudCreationAnimator] ✓ Animation de création de boue terminée — contrôle redonné");
    }
}
