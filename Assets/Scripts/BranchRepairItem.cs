using UnityEngine;

/// <summary>
/// Composant à ajouter à une branche pour la rendre réparatrice de barrage
/// Gère l'interaction avec le système de réparation du barrage
/// Fonctionne avec n'importe quel type de branche (Poplar, Birch, etc.)
/// </summary>
public class BranchRepairItem : MonoBehaviour
{
    [SerializeField] private DamManager damManager;
    
    [Tooltip("Distance maximum pour valider la réparation si le trigger physique échoue au lâcher")]
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
        {
            damManager = FindObjectOfType<DamManager>();
        }

        if (debugLogs)
        {
            if (damManager == null)
                Debug.LogWarning($"[BranchRepairItem] No DamManager found for branch at {gameObject.name}");
            
            if (branchItem != null)
                Debug.Log($"[BranchRepairItem] Branch initialized - Type: {branchItem.GetTreeTypeName()}");
        }
    }

    private void Update()
    {
        // Détecte quand on dépose une branche
        // On vérifie si un PlayerController existe dans les parents pour savoir si elle est tenue
        bool isCurrentlyEquipped = GetComponentInParent<TopDownPlayerController>() != null;
        
        if (wasPickedUpLastFrame && !isCurrentlyEquipped)
        {
            // Sécurité : Si le trigger n'a pas détecté la zone (car le collider est souvent désactivé quand on porte l'objet)
            // on force la détection par distance si on est proche du DamManager
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
            {
                if (debugLogs)
                    Debug.Log($"[BranchRepairItem] ✓ Réparation activée au lâcher : {gameObject.name}");
                
                OnRepairZoneDeposit();
            }
            else if (debugLogs)
            {
                Debug.Log($"[BranchRepairItem] ! Branche lâchée trop loin du barrage");
            }
        }
        
        wasPickedUpLastFrame = isCurrentlyEquipped;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Vérifie si on entre en collision avec le DamManager/Barrage
        DamManager foundDam = collision.GetComponentInParent<DamManager>();
        if (foundDam != null)
        {
            damManager = foundDam;
            isInRepairZone = true;
            
            if (debugLogs)
                Debug.Log($"[BranchRepairItem] -> Entré dans la zone du barrage (Détecté via : {collision.gameObject.name})");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Vérifie si on sort du DamManager/Barrage
        if (collision.GetComponentInParent<DamManager>() != null)
        {
            isInRepairZone = false;
            
            if (debugLogs)
                Debug.Log($"[BranchRepairItem] <- Sorti de la zone du barrage");
        }
    }

    /// <summary>
    /// À appeler quand la branche est déposée dans la zone de réparation
    /// </summary>
    private void OnRepairZoneDeposit()
    {
        if (!isInRepairZone || damManager == null) return;

        if (debugLogs && branchItem != null)
            Debug.Log($"[BranchRepairItem] {branchItem.GetTreeTypeName()} branch used to repair dam!");
        
        damManager.RepairDamWithBranch(this);
    }

    public bool IsInRepairZone => isInRepairZone;
    public BranchItem GetBranchItem() => branchItem;
}
