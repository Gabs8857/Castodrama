using UnityEngine;

/// <summary>
/// Composant à ajouter à une branche pour la rendre réparatrice de barrage
/// Gère l'interaction avec le système de réparation du barrage
/// Fonctionne avec n'importe quel type de branche (Poplar, Birch, etc.)
/// </summary>
public class BranchRepairItem : MonoBehaviour
{
    [SerializeField] private DamManager damManager;
    
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
        // Si la branche n'a pas de parent (ou son parent n'a pas TopDownPlayerController), elle est déposée
        bool isCurrentlyEquipped = transform.parent != null && 
                                   transform.parent.GetComponent<TopDownPlayerController>() != null;
        
        if (wasPickedUpLastFrame && !isCurrentlyEquipped && isInRepairZone)
        {
            if (debugLogs)
                Debug.Log($"[BranchRepairItem] ✓ Branch dropped in repair zone!");
            // La branche vient d'être déposée dans la zone de réparation
            OnRepairZoneDeposit();
        }
        
        if (debugLogs && wasPickedUpLastFrame && !isCurrentlyEquipped)
        {
            Debug.Log($"[BranchRepairItem] Branch dropped but isInRepairZone={isInRepairZone}");
        }
        
        wasPickedUpLastFrame = isCurrentlyEquipped;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Vérifie si on entre en collision avec le DamManager/Barrage
        if (collision.GetComponent<DamManager>() != null)
        {
            isInRepairZone = true;
            
            if (debugLogs)
                Debug.Log($"[BranchRepairItem] Branch entered Barrage zone");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Vérifie si on sort du DamManager/Barrage
        if (collision.GetComponent<DamManager>() != null)
        {
            isInRepairZone = false;
            
            if (debugLogs)
                Debug.Log($"[BranchRepairItem] Branch left Barrage zone");
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
