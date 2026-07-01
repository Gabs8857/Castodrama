using UnityEngine;

public class WaterSceneTransition : MonoBehaviour
{
    [Header("🌊 Parents à gérer")]
    [Tooltip("Assigne le GameObject 'Surface' (contient tous les éléments de surface)")]
    [SerializeField] private GameObject surfaceParent;  // ✅ NOUVEAU: Parent "Surface"
    [Tooltip("Assigne le GameObject 'Fond' (contient FondRivière, Terrier, etc.)")]
    [SerializeField] private GameObject fondParent;      // ✅ NOUVEAU: Parent "Fond"

    [Header("🔧 Paramètres")]
    [SerializeField] private bool debugLogs = true;
    private float lastHandleTime = -1f;
    private const float HANDLE_COOLDOWN = 1f;

    private void Start()
    {
        // Auto-detection des parents si non assignés
        if (surfaceParent == null)
        {
            surfaceParent = GameObject.Find("Surface");
            if (surfaceParent != null && debugLogs)
                Debug.Log("[WaterSceneTransition] ✓ Found Surface parent");
        }

        if (fondParent == null)
        {
            fondParent = GameObject.Find("Fond");
            if (fondParent != null && debugLogs)
                Debug.Log("[WaterSceneTransition] ✓ Found Fond parent");
        }
    }

    /// <summary>
    /// Active/Désactive RÉCURSIVEMENT un GameObject et tous ses enfants.
    /// </summary>
    private void SetActiveRecursively(GameObject parent, bool active)
    {
        if (parent == null) return;

        parent.SetActive(active);

        // Gère tous les enfants récursivement
        foreach (Transform child in parent.transform)
        {
            SetActiveRecursively(child.gameObject, active);
        }

        if (debugLogs)
            Debug.Log($"[WaterSceneTransition] {'✅'}/{'❌'} {parent.name} + all children SetActive({active})");
    }

    /// <summary> Vérifie si un GameObject est un enfant (direct ou indirect) de parent. </summary>
    private bool IsChildOf(GameObject child, GameObject parent)
    {
        if (parent == null) return false;
        Transform current = child.transform;
        while (current != null)
        {
            if (current.gameObject == parent) return true;
            current = current.parent;
        }
        return false;
    }

    /// <summary> Appelé quand le joueur entre dans l'eau (TP vers la rivière) </summary>
    public void OnEnterWater()
    {
        if (debugLogs) Debug.Log("[WaterSceneTransition] 🌊 OnEnterWater");

        // ✅ Désactive TOUTE la Surface (arbres, tilemap, zones, etc.)
        SetActiveRecursively(surfaceParent, false);

        // ✅ Active TOUT le Fond (FondRivière, Terrier, etc.)
        SetActiveRecursively(fondParent, true);

        // Force le mode nage profonde
        CharacterAnimator animator = FindObjectOfType<CharacterAnimator>();
        if (animator != null)
        {
            animator.StartSwimmingDeep();
            if (debugLogs) Debug.Log("[WaterSceneTransition] ✓ Deep swim started");
        }
    }

    /// <summary> Appelé quand le joueur sort de l'eau (TP vers l'extérieur) </summary>
    public void OnExitWater()
    {
        if (debugLogs) Debug.Log("[WaterSceneTransition] 🏝️ OnExitWater");

        // ✅ Active TOUTE la Surface
        SetActiveRecursively(surfaceParent, true);

        // ✅ Désactive TOUT le Fond
        SetActiveRecursively(fondParent, false);

        // Force le mode marche
        CharacterAnimator animator = FindObjectOfType<CharacterAnimator>();
        if (animator != null)
        {
            animator.StopSwimmingDeep();
            if (debugLogs) Debug.Log("[WaterSceneTransition] ✓ Deep swim stopped");
        }
    }

    /// <summary> Appelé lors d'un TP pour identifier la destination </summary>
    public void HandleTeleportToDestination(Transform destination)
    {
        if (Time.time < lastHandleTime + HANDLE_COOLDOWN)
        {
            if (debugLogs) Debug.Log("[WaterSceneTransition] ⏳ Cooldown actif - ignoré");
            return;
        }
        lastHandleTime = Time.time;

        if (destination == null) return;

        string destName = destination.name.ToLower();
        if (debugLogs) Debug.Log($"[WaterSceneTransition] TP vers: {destination.name}");

        // Cas spécial: Retour du terrier (TP_INT)
        if (destName.Contains("int") || destName.Contains("enhutte"))
        {
            if (debugLogs) Debug.Log("[WaterSceneTransition] ✅ Retour du terrier → mode profondeur");
            ForceDeepSwimMode();
            return;
        }

        // Vérifie si le joueur est dans la zone Surface après TP
        TopDownPlayerController player = FindObjectOfType<TopDownPlayerController>();
        bool isInSurfaceZone = false;

        if (player != null && surfaceParent != null)
        {
            Collider2D[] overlaps = Physics2D.OverlapPointAll(player.transform.position);
            foreach (Collider2D col in overlaps)
            {
                if (IsChildOf(col.gameObject, surfaceParent))
                {
                    isInSurfaceZone = true;
                    if (debugLogs) Debug.Log("[WaterSceneTransition] ✅ Joueur dans Surface après TP");
                    break;
                }
            }
        }

        // Si destination = eau/rivière OU joueur dans Surface → entre dans l'eau
        if (destName.Contains("eau") || destName.Contains("rivière") || destName.Contains("water") || isInSurfaceZone)
        {
            OnEnterWater();
        }
        else
        {
            OnExitWater();
        }
    }

    /// <summary> Force le mode profondeur (retour du terrier) </summary>
    private void ForceDeepSwimMode()
    {
        if (debugLogs) Debug.Log("[WaterSceneTransition] 💥 ForceDeepSwimMode");

        SetActiveRecursively(surfaceParent, false);
        SetActiveRecursively(fondParent, true);

        CharacterAnimator animator = FindObjectOfType<CharacterAnimator>();
        if (animator != null) animator.StartSwimmingDeep();

        RiverBottomTeleport riverBottom = FindObjectOfType<RiverBottomTeleport>();
        if (riverBottom != null) riverBottom.ForceZoneEntry();
    }

    /// <summary> Force le retour à la surface </summary>
    private void ForceReturnToSurfaceMode()
    {
        if (debugLogs) Debug.Log("[WaterSceneTransition] 🌿 ForceReturnToSurfaceMode");

        SetActiveRecursively(surfaceParent, true);
        SetActiveRecursively(fondParent, false);

        CharacterAnimator animator = FindObjectOfType<CharacterAnimator>();
        if (animator != null && animator.IsSwimmingDeep)
            animator.StopSwimmingDeep();

        RiverBottomTeleport riverBottom = FindObjectOfType<RiverBottomTeleport>();
        if (riverBottom != null)
            riverBottom.HandleWaterSceneTransition();
    }
}