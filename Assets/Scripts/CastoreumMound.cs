using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

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
    private bool playerInRange = false;

    // ========== START ==========
    private void Start()
    {
        if (moundLight == null)
            moundLight = GetComponentInChildren<Light2D>();

        // 🔍 VÉRIFIE LE COLLIDER
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Debug.Log($"[CastoreumMound] {gameObject.name} | Collider: {col.name} | isTrigger: {col.isTrigger} | Type: {col.GetType().Name}");
        }
        else
        {
            Debug.LogError($"[CastoreumMound] ❌ {gameObject.name} | AUCUN COLLIDER TROUVÉ !");
        }

        // 🔍 VÉRIFIE LA LUMIÈRE
        if (moundLight != null)
            Debug.Log($"[CastoreumMound] {gameObject.name} | Light2D trouvée: {moundLight.name}");
        else
            Debug.LogError($"[CastoreumMound] ❌ {gameObject.name} | AUCUNE LIGHT2D TROUVÉE !");

        UpdateLightColor();
    }

    // ========== UPDATE ==========
    private void Update()
    {
        // 🔍 VÉRIFIE LE JOUEUR
        if (playerTransform == null)
        {
            playerTransform = FindObjectOfType<TopDownPlayerController>()?.transform;
            if (playerTransform == null)
            {
                Debug.LogError($"[CastoreumMound] ❌ {gameObject.name} | TopDownPlayerController INTROUVABLE !");
                return;
            }
            else
            {
                Debug.Log($"[CastoreumMound] {gameObject.name} | Joueur trouvé: {playerTransform.name}");
            }
        }

        // 🔍 AFFICHE LA DISTANCE EN TEMPS RÉEL
        float distance = Vector2.Distance(transform.position, playerTransform.position);
        bool newPlayerInRange = distance <= interactionRange;

        if (newPlayerInRange != playerInRange)
        {
            playerInRange = newPlayerInRange;
            Debug.Log($"[CastoreumMound] {gameObject.name} | " +
                      $"Distance: {distance:F2} | " +
                      $"Range: {interactionRange} | " +
                      $"InRange: {playerInRange}");
        }

        // 🔍 VÉRIFIE L'APPAREIL SUR E
        if (playerInRange && InputHelper.InteractPressed())
        {
            Debug.Log($"[CastoreumMound] {gameObject.name} | 🎮 INTERACT PRESSÉ ! | ClaimProgress: {claimProgress:P0}");
            if (claimProgress < 1f)
            {
                isClaiming = true;
            }
        }

        // 🔍 CLAIM EN COURS
        if (isClaiming && playerInRange)
        {
            claimProgress = Mathf.Min(1f, claimProgress + claimSpeed * Time.deltaTime);
            UpdateLightColor();
            Debug.Log($"[CastoreumMound] {gameObject.name} | Claim: {claimProgress:P0}");
        }
        else if (isClaiming && !playerInRange)
        {
            Debug.LogWarning($"[CastoreumMound] {gameObject.name} | ⚠️ Claim interrompu (joueur trop loin) !");
            isClaiming = false;
        }
    }

    // ========== TRIGGERS ==========
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[CastoreumMound] {gameObject.name} | 🔘 OnTriggerEnter2D avec: {other.name} | Tag: {other.tag} | Layer: {LayerMask.LayerToName(other.gameObject.layer)}");

        if (IsPlayer(other))
        {
            Debug.Log($"[CastoreumMound] {gameObject.name} | ✅ CONTACT AVEC LE JOUEUR !");
        }
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
        {
            Debug.Log($"[CastoreumMound] {gameObject.name} | Joueur détecté via: {(byTag ? "TAG" : "COMPOSANT")}");
        }
        return isPlayer;
    }

    private void UpdateLightColor()
    {
        if (moundLight != null)
        {
            moundLight.color = Color.Lerp(unclaimedColor, claimedColor, claimProgress);
        }
    }

    public float GetClaimProgress() => claimProgress;
    public bool IsFullyClaimed() => claimProgress >= 1f;
}