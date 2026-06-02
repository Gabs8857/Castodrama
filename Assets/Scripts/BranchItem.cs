using UnityEngine;

/// <summary>
/// Types d'arbres disponibles dans le jeu
/// </summary>
public enum TreeType
{
    Poplar,  // Peuplier
    Birch    // Bouleau
}

/// <summary>
/// Classe de base pour les branches d'arbre
/// Gère le type d'arbre et l'interaction avec le système de réparation
/// </summary>
public class BranchItem : MonoBehaviour
{
    [SerializeField] protected TreeType treeType = TreeType.Poplar;
    
    [Header("Debug")]
    [SerializeField] protected bool debugLogs = true;

    protected EquippableItem equippableItem;

    protected virtual void Awake()
    {
        // Récupère les composants
        equippableItem = GetComponent<EquippableItem>();
        
        if (equippableItem == null && debugLogs)
            Debug.LogWarning($"[BranchItem] No EquippableItem found on {gameObject.name}!");
    }

    protected virtual void Start()
    {
        if (debugLogs)
            Debug.Log($"[BranchItem] Branch spawned - Type: {treeType}, Name: {gameObject.name}");
    }

    /// <summary>
    /// Retourne le type d'arbre de cette branche
    /// </summary>
    public TreeType GetTreeType()
    {
        return treeType;
    }

    /// <summary>
    /// Retourne le nom du type d'arbre
    /// </summary>
    public string GetTreeTypeName()
    {
        return treeType switch
        {
            TreeType.Poplar => "Poplar",
            TreeType.Birch => "Birch",
            _ => "Unknown"
        };
    }
}
