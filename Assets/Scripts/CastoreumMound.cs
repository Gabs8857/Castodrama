using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Butte à castoréum que le joueur peut marquer (claim) en restant à proximité
/// et en maintenant l'interaction. Pendant le claim, le joueur est figé en
/// animation "Castoreum" via CastoreumAnimator (mouvement bloqué).
/// </summary>
public class CastoreumMound : MonoBehaviour
{
    [Header("💡 Lumière")]
    [SerializeField] private Light2D moundLight;
    [SerializeField] private Color unclaimedColor = Color.red;
    [SerializeField] private Color claimedColor = Color.green;

    [Header("⚙️ Claim")]
    [Range(0f, 1f)]
    [SerializeField] private float claimProgress = 0f;
    [SerializeField] private float claimSpeed = 0.1f;

    [Header("🎮 Interaction")]
    [SerializeField] private float interactionRange = 1.5f;

    [Header("🐛 DEBUG")]
    [SerializeField] private bool debugLogs = true;

    private bool isClaiming = false;
    private Transform playerTransform;
    private CastoreumAnimator playerCastoreumAnimator;
    private bool playerInRange = false;

    // ========== START ==========
    private void Start()
    {
        if (moundLight == null)
            moundLight = GetComponentInChildren<Light2D>();

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            Debug.Log($"[CastoreumMound] {gameObject.name} | Collider: {col.name} | isTrigger: {col.isTrigger} | Type: {col.GetType().Name}");
        else
            Debug.LogError($"[CastoreumMound] ❌ {gameObject.name} | AUCUN COLLIDER TROUVÉ !");

        if (moundLight != null)
            Debug.Log($"[CastoreumMound] {gameObject.name} | Light2D trouvée: {moundLight.name}");
        else
            Debug.LogError($"[CastoreumMound] ❌ {gameObject.name} | AUCUNE LIGHT2D TROUVÉE !");

        UpdateLightColor();
    }

    // ========== UPDATE ==========
    private void Update()
    {
        if (playerTransform == null)
        {
            TopDownPlayerController controller = FindObjectOfType<TopDownPlayerController>();
            if (controller == null)
            {
                Debug.LogError($"[CastoreumMound] ❌ {gameObject.name} | TopDownPlayerController INTROUVABLE !");
                return;
            }

            playerTransform = controller.transform;
            playerCastoreumAnimator = controller.GetComponent<CastoreumAnimator>();

            if (playerCastoreumAnimator == null)
                Debug.LogWarning($"[CastoreumMound] ⚠ CastoreumAnimator introuvable sur le joueur !");

            Debug.Log($"[CastoreumMound] {gameObject.name} | Joueur trouvé: {playerTransform.name}");
        }

        float distance = Vector2.Distance(transform.position, playerTransform.position);
        bool newPlayerInRange = distance <= interactionRange;

        if (newPlayerInRange != playerInRange)
        {
            playerInRange = newPlayerInRange;
            Debug.Log($"[CastoreumMound] {gameObject.name} | Distance: {distance:F2} | Range: {interactionRange} | InRange: {playerInRange}");

            // Si le joueur sort de la zone, on coupe le claim proprement
            if (!playerInRange && isClaiming)
                StopClaim();
        }

        // Démarrage du claim
        if (playerInRange && InputHelper.InteractPressed() && claimProgress < 1f && !isClaiming)
        {
            Debug.Log($"[CastoreumMound] {gameObject.name} | 🎮 INTERACT PRESSÉ ! | ClaimProgress: {claimProgress:P0}");
            StartClaim();
        }

        // Claim en cours
        if (isClaiming && playerInRange)
        {
            claimProgress = Mathf.Min(1f, claimProgress + claimSpeed * Time.deltaTime);
            UpdateLightColor();
            Debug.Log($"[CastoreumMound] {gameObject.name} | Claim: {claimProgress:P0}");

            if (claimProgress >= 1f)
            {
                Debug.Log($"[CastoreumMound] {gameObject.name} | ✓✓✓ BUTTE ENTIÈREMENT MARQUÉE ✓✓✓");
                StopClaim();
            }
        }
        else if (isClaiming && !playerInRange)
        {
            Debug.LogWarning($"[CastoreumMound] {gameObject.name} | ⚠️ Claim interrompu (joueur trop loin) !");
            StopClaim();
        }
    }

    private void StartClaim()
    {
        isClaiming = true;

        if (playerCastoreumAnimator != null)
            playerCastoreumAnimator.StartClaiming();
    }

    private void StopClaim()
    {
        isClaiming = false;

        if (playerCastoreumAnimator != null)
            playerCastoreumAnimator.StopClaiming();
    }

    // ========== TRIGGERS ==========
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[CastoreumMound] {gameObject.name} | 🔘 OnTriggerEnter2D avec: {other.name} | Tag: {other.tag} | Layer: {LayerMask.LayerToName(other.gameObject.layer)}");

        if (IsPlayer(other))
            Debug.Log($"[CastoreumMound] {gameObject.name} | ✅ CONTACT AVEC LE JOUEUR !");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log($"[CastoreumMound] {gameObject.name} | 🔙 OnTriggerExit2D avec: {other.name}");
    }

    // ========== UTILITAIRES ==========
    private bool IsPlayer(Collider2D collider)
    {
        bool byTag = collider.CompareTag("Player");
        bool byComponent = collider.GetComponent<TopDownPlayerController>() != null;
        bool isPlayer = byTag || byComponent;

        if (debugLogs && isPlayer)
            Debug.Log($"[CastoreumMound] {gameObject.name} | Joueur détecté via: {(byTag ? "TAG" : "COMPOSANT")}");

        return isPlayer;
    }

    private void UpdateLightColor()
    {
        if (moundLight != null)
            moundLight.color = Color.Lerp(unclaimedColor, claimedColor, claimProgress);
    }

    public float GetClaimProgress() => claimProgress;
    public bool IsFullyClaimed() => claimProgress >= 1f;
}
