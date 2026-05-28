using UnityEngine;

/// <summary>
/// Composant à ajouter à une branche pour la rendre réparatrice de barrage
/// Gère l'interaction avec le système de réparation du barrage
/// </summary>
public class BranchRepairItem : MonoBehaviour
{
    [SerializeField] private DamManager damManager;
    
    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private EquippableItem equippableItem;
    private bool isInRepairZone = false;

    private void Start()
    {
        equippableItem = GetComponent<EquippableItem>();
        
        if (damManager == null)
        {
            damManager = FindObjectOfType<DamManager>();
        }

        if (debugLogs && damManager == null)
        {
            Debug.LogWarning($"[BranchRepairItem] No DamManager found for branch at {gameObject.name}");
        }
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
    public void OnRepairZoneDeposit()
    {
        if (!isInRepairZone || damManager == null) return;

        damManager.RepairDamWithBranch(this);
        
        if (debugLogs)
            Debug.Log($"[BranchRepairItem] Branch used to repair dam!");
    }

    public bool IsInRepairZone => isInRepairZone;
}
