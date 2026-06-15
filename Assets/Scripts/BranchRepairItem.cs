using UnityEngine;

/// <summary>
/// Composant à ajouter à une branche pour la rendre réparatrice de barrage.
/// Répare la fissure active LA PLUS PROCHE de l'endroit où la branche est déposée.
/// </summary>
public class BranchRepairItem : MonoBehaviour
{
    [SerializeField] private DamManager damManager;

    [Tooltip("Distance maximum pour valider la réparation au lâcher")]
    [SerializeField] private float repairDistanceThreshold = 5f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private EquippableItem equippableItem;
    private BranchItem branchItem;
    private bool isInRepairZone = false;
    private bool wasPickedUpLastFrame = false;

    private void Start()
    {
        equippableItem = GetComponent<EquippableItem>();
        branchItem = GetComponent<BranchItem>();

        if (damManager == null)
            damManager = FindObjectOfType<DamManager>();

        if (debugLogs)
        {
            if (damManager == null)
                Debug.LogWarning($"[BranchRepairItem] No DamManager found for {gameObject.name}");
            if (branchItem != null)
                Debug.Log($"[BranchRepairItem] Branch initialized - Type: {branchItem.GetTreeTypeName()}");
        }
    }

    private void Update()
    {
        bool isCurrentlyEquipped = GetComponentInParent<TopDownPlayerController>() != null;

        if (wasPickedUpLastFrame && !isCurrentlyEquipped)
        {
            // Fallback distance si le trigger n'a pas détecté la zone
            if (!isInRepairZone && damManager != null)
            {
                float dist = Vector2.Distance(transform.position, damManager.transform.position);
                if (dist <= repairDistanceThreshold)
                {
                    isInRepairZone = true;
                    if (debugLogs) Debug.Log($"[BranchRepairItem] Zone validée par proximité ({dist:F1}m)");
                }
            }

            if (isInRepairZone)
                TryRepairNearestCrack();
            else if (debugLogs)
                Debug.Log("[BranchRepairItem] Branche lâchée trop loin du barrage.");
        }

        wasPickedUpLastFrame = isCurrentlyEquipped;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponentInParent<DamManager>() != null)
        {
            damManager = collision.GetComponentInParent<DamManager>();
            isInRepairZone = true;
            if (debugLogs)
                Debug.Log($"[BranchRepairItem] → Entré dans la zone du barrage ({collision.gameObject.name})");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponentInParent<DamManager>() != null)
        {
            isInRepairZone = false;
            if (debugLogs)
                Debug.Log("[BranchRepairItem] ← Sorti de la zone du barrage");
        }
    }

    /// <summary>
    /// Trouve la fissure active la plus proche de là où la branche est déposée
    /// et demande au DamManager de la réparer.
    /// </summary>
    private void TryRepairNearestCrack()
    {
        if (damManager == null) return;
        if (damManager.GetCurrentCrackCount() == 0)
        {
            if (debugLogs) Debug.Log("[BranchRepairItem] Aucune fissure active à réparer.");
            return;
        }

        int nearestIndex = damManager.GetNearestActiveCrackIndex(transform.position);

        if (nearestIndex < 0)
        {
            if (debugLogs) Debug.Log("[BranchRepairItem] Aucune fissure trouvée.");
            return;
        }

        if (debugLogs)
        {
            string type = branchItem != null ? branchItem.GetTreeTypeName() : "?";
            Debug.Log($"[BranchRepairItem] ✓ Réparation fissure #{nearestIndex} avec branche {type}");
        }

        damManager.RepairCrackAtIndex(nearestIndex, this);
    }

    public bool IsInRepairZone => isInRepairZone;
    public BranchItem GetBranchItem() => branchItem;
}
