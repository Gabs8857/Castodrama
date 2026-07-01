using UnityEngine;
using UnityEngine.U2D.Animation;

/// <summary>
/// Gère l'animation et le blocage de mouvement pendant que le joueur "claim" le castoréum.
/// Pendant ce mode :
///   - L'animation passe en catégorie "Castoreum" (équivalent visuel de Walk_Up, mais figé)
///   - Le Rigidbody2D est mis à vitesse zéro, le mouvement est ignoré
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class CastoreumAnimator : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private string castoreumCategoryName = "Castoreum";
    [SerializeField] private string[] castoreumFrameNames = { "Frame1", "Frame2", "Frame3" };
    [SerializeField] private float frameSwitchSpeed = 0.1f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private SpriteResolver spriteResolver;
    private Rigidbody2D rb;
    private PlayerController playerController;

    private bool isClaiming = false;
    private float timeSinceLastSwitch = 0f;
    private int currentFrameIndex = 0;
    private Vector2 lockedPosition;

    public bool IsClaiming => isClaiming;

    private void Awake()
    {
        spriteResolver = GetComponent<SpriteResolver>();
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();

        // 🔍 DIAGNOSTIC COMPLET au démarrage
        Debug.Log("═══════════════════════════════════════");
        Debug.Log($"[CastoreumAnimator] INIT sur '{gameObject.name}'");
        Debug.Log($"[CastoreumAnimator] SpriteResolver: {(spriteResolver != null ? "✓ TROUVÉ" : "❌ MANQUANT")}");
        Debug.Log($"[CastoreumAnimator] Rigidbody2D: {(rb != null ? "✓ TROUVÉ" : "❌ MANQUANT")}");
        Debug.Log($"[CastoreumAnimator] PlayerController: {(playerController != null ? $"✓ TROUVÉ ({playerController.GetType().Name})" : "❌ MANQUANT")}");
        Debug.Log($"[CastoreumAnimator] Catégorie configurée: '{castoreumCategoryName}'");
        Debug.Log($"[CastoreumAnimator] Frames configurées: [{string.Join(", ", castoreumFrameNames)}]");

        if (spriteResolver == null)
            Debug.LogError($"[CastoreumAnimator] ❌ CRITIQUE: Pas de SpriteResolver sur {gameObject.name} ! L'animation ne pourra jamais s'afficher.");

        if (playerController == null)
            Debug.LogWarning($"[CastoreumAnimator] ⚠ Pas de PlayerController trouvé — le blocage de mouvement via enabled=false ne fonctionnera pas (seul le Rigidbody sera bloqué).");

        // Vérifie que la catégorie/label existe vraiment dans la Sprite Library
        if (spriteResolver != null && castoreumFrameNames.Length > 0)
        {
            TestCategoryExists();
        }

        Debug.Log("═══════════════════════════════════════");
    }

    /// <summary>
    /// Tente d'appliquer la première frame Castoreum immédiatement (sans activer le claim)
    /// pour vérifier dans la Console si la catégorie/label existe dans la Sprite Library.
    /// </summary>
    private void TestCategoryExists()
    {
        try
        {
            // On sauvegarde l'état actuel pour le restaurer juste après (test non-destructif)
            string currentCategory = spriteResolver.GetCategory();
            string currentLabel = spriteResolver.GetLabel();

            spriteResolver.SetCategoryAndLabel(castoreumCategoryName, castoreumFrameNames[0]);
            string appliedCategory = spriteResolver.GetCategory();
            string appliedLabel = spriteResolver.GetLabel();

            bool success = appliedCategory == castoreumCategoryName && appliedLabel == castoreumFrameNames[0];

            if (success)
                Debug.Log($"[CastoreumAnimator] ✓ Catégorie '{castoreumCategoryName}' / Label '{castoreumFrameNames[0]}' EXISTE dans la Sprite Library.");
            else
                Debug.LogError($"[CastoreumAnimator] ❌ Catégorie '{castoreumCategoryName}' / Label '{castoreumFrameNames[0]}' INTROUVABLE dans la Sprite Library ! "
                    + $"Résultat obtenu : catégorie='{appliedCategory}', label='{appliedLabel}'. "
                    + "Vérifie l'orthographe exacte (sensible à la casse) dans ta Sprite Library Asset.");

            // Restaure l'état d'origine pour ne pas perturber visuellement avant le vrai claim
            if (!string.IsNullOrEmpty(currentCategory))
                spriteResolver.SetCategoryAndLabel(currentCategory, currentLabel);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[CastoreumAnimator] ❌ Exception lors du test de catégorie : {e.Message}");
        }
    }

    private void Update()
    {
        if (!isClaiming) return;

        if (debugLogs && Time.frameCount % 30 == 0)
            Debug.Log($"[CastoreumAnimator] Update actif — isClaiming=true, frame actuelle={currentFrameIndex}, rb.velocity={(rb != null ? rb.linearVelocity.ToString() : "NULL")}");

        // Verrouille le mouvement à chaque frame tant que le claim est actif
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            // Fixe la position en dur — au cas où une vélocité résiduelle ou
            // un autre script déplace encore le Rigidbody malgré la vélocité à zéro
            if (rb.position != lockedPosition)
            {
                rb.position = lockedPosition;
                if (debugLogs && Time.frameCount % 30 == 0)
                    Debug.LogWarning($"[CastoreumAnimator] ⚠ Position corrigée — le joueur tentait de se déplacer pendant le claim (delta détecté).");
            }
        }
        else if (debugLogs)
        {
            Debug.LogWarning("[CastoreumAnimator] ⚠ rb est NULL pendant le claim, impossible de bloquer le mouvement !");
        }

        if (spriteResolver != null && castoreumFrameNames.Length > 0)
        {
            timeSinceLastSwitch += Time.deltaTime;
            if (timeSinceLastSwitch >= frameSwitchSpeed)
            {
                timeSinceLastSwitch = 0f;
                SwitchFrame();
            }
        }
        else if (debugLogs)
        {
            Debug.LogWarning($"[CastoreumAnimator] ⚠ Animation ignorée — spriteResolver null: {spriteResolver == null}, frameNames vide: {castoreumFrameNames.Length == 0}");
        }
    }

    private void FixedUpdate()
    {
        if (isClaiming && rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.position = lockedPosition;
        }
    }

    private void SwitchFrame()
    {
        string label = castoreumFrameNames[currentFrameIndex];

        try
        {
            spriteResolver.SetCategoryAndLabel(castoreumCategoryName, label);

            if (debugLogs)
                Debug.Log($"[CastoreumAnimator] 🎬 Frame appliquée : '{castoreumCategoryName}' / '{label}' (index {currentFrameIndex})");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[CastoreumAnimator] ❌ Erreur SetCategoryAndLabel('{castoreumCategoryName}', '{label}'): {e.Message}");
        }

        currentFrameIndex = (currentFrameIndex + 1) % castoreumFrameNames.Length;
    }

    /// <summary>
    /// Démarre le mode claim : bloque le mouvement et active l'animation Castoreum.
    /// </summary>
    public void StartClaiming()
    {
        Debug.Log($"[CastoreumAnimator] >>> StartClaiming() appelé. isClaiming actuel = {isClaiming}");

        if (isClaiming)
        {
            Debug.LogWarning("[CastoreumAnimator] StartClaiming() ignoré — déjà en train de claim.");
            return;
        }

        isClaiming = true;
        currentFrameIndex = 0;
        timeSinceLastSwitch = 0f;

        if (rb != null)
        {
            lockedPosition = rb.position;
            rb.linearVelocity = Vector2.zero;
            Debug.Log($"[CastoreumAnimator] ✓ Position verrouillée à {lockedPosition}, vélocité mise à zéro.");
        }
        else
        {
            Debug.LogError("[CastoreumAnimator] ❌ Impossible de bloquer le mouvement — rb est NULL !");
        }

        if (playerController != null)
        {
            playerController.enabled = false;
            Debug.Log($"[CastoreumAnimator] ✓ PlayerController ({playerController.GetType().Name}) désactivé.");
        }
        else
        {
            Debug.LogError("[CastoreumAnimator] ❌ Impossible de désactiver le PlayerController — référence NULL ! "
                + "Le joueur pourra probablement encore bouger.");
        }

        if (spriteResolver != null && castoreumFrameNames.Length > 0)
        {
            spriteResolver.SetCategoryAndLabel(castoreumCategoryName, castoreumFrameNames[0]);
            Debug.Log($"[CastoreumAnimator] ✓ Première frame appliquée : '{castoreumCategoryName}'/'{castoreumFrameNames[0]}'");
        }
        else
        {
            Debug.LogError($"[CastoreumAnimator] ❌ Impossible d'appliquer l'animation — spriteResolver null: {spriteResolver == null}, frames vides: {castoreumFrameNames.Length == 0}");
        }

        Debug.Log("[CastoreumAnimator] 🦫 Mode CLAIM CASTOREUM activé — isClaiming = true");
    }

    /// <summary>
    /// Arrête le mode claim : réactive le mouvement normal.
    /// </summary>
    public void StopClaiming()
    {
        Debug.Log($"[CastoreumAnimator] >>> StopClaiming() appelé. isClaiming actuel = {isClaiming}");

        if (!isClaiming)
        {
            Debug.LogWarning("[CastoreumAnimator] StopClaiming() ignoré — pas en train de claim.");
            return;
        }

        isClaiming = false;

        if (playerController != null)
        {
            playerController.enabled = true;
            Debug.Log($"[CastoreumAnimator] ✓ PlayerController ({playerController.GetType().Name}) réactivé.");
        }

        Debug.Log("[CastoreumAnimator] ✓ Fin du claim — isClaiming = false, mouvement débloqué");
    }
}
