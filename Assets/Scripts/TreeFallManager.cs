using UnityEngine;

/// <summary>
/// Gère la chute d'arbre et la génération de branches
/// Spawn des branches quand la 2e cassure du barrage apparaît
/// Support pour plusieurs types d'arbres (Poplar, Birch)
/// </summary>
public class TreeFallManager : MonoBehaviour
{
    [Header("Branch Spawning")]
    [SerializeField] private GameObject poplarBranchPrefab;
    [SerializeField] private GameObject birchBranchPrefab;
    [SerializeField] private int poplarBranchCount = 4;
    [SerializeField] private int birchBranchCount = 4;
    [SerializeField] private float spawnRadius = 2f;
    [SerializeField] private Vector2 spawnCenterOffset = new Vector2(0, -3f);
    
    [Header("References")]
    [SerializeField] private DamManager damManager;
    
    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private int lastCrackCount = 0;
    private bool hasSpawnedBranchesAt2 = false;

    private void Start()
    {
        if (debugLogs)
        {
            Debug.Log("═══════════════════════════════════════");
            Debug.Log("[TreeFallManager] INITIALIZING");
            Debug.Log("═══════════════════════════════════════");
        }

        if (damManager == null)
        {
            damManager = FindObjectOfType<DamManager>();
            if (debugLogs && damManager != null)
                Debug.Log("[TreeFallManager] ✓ DamManager found automatically");
        }
        else if (debugLogs)
            Debug.Log("[TreeFallManager] ✓ DamManager assigned");

        if (debugLogs)
        {
            Debug.Log($"[TreeFallManager] poplarBranchPrefab: {(poplarBranchPrefab != null ? "✓ ASSIGNED" : "✗ NOT ASSIGNED")}");
            Debug.Log($"[TreeFallManager] birchBranchPrefab: {(birchBranchPrefab != null ? "✓ ASSIGNED" : "✗ NOT ASSIGNED")}");
            Debug.Log($"[TreeFallManager] damManager: {(damManager != null ? "✓ ASSIGNED" : "✗ NOT ASSIGNED")}");
            Debug.Log($"[TreeFallManager] poplarBranchCount: {poplarBranchCount}");
            Debug.Log($"[TreeFallManager] birchBranchCount: {birchBranchCount}");
            Debug.Log($"[TreeFallManager] spawnRadius: {spawnRadius}");
            Debug.Log("═══════════════════════════════════════");
        }

        lastCrackCount = 0;
    }

    private void Update()
    {
        if (damManager == null) return;

        int currentCrackCount = damManager.GetCurrentCrackCount();

        // Debug chaque frame pour voir les cracks
        if (debugLogs && currentCrackCount != lastCrackCount)
        {
            Debug.Log($"[TreeFallManager] Crack count changed: {lastCrackCount} → {currentCrackCount}");
        }

        // Vérifie si on vient de passer à 2 fissures
        if (currentCrackCount == 2 && !hasSpawnedBranchesAt2)
        {
            if (debugLogs)
            {
                Debug.Log("[TreeFallManager] ✓✓✓ 2ème cassure DÉTECTÉE! ✓✓✓");
                Debug.Log($"[TreeFallManager] Current crack count: {currentCrackCount}");
                Debug.Log($"[TreeFallManager] hasSpawnedBranchesAt2 before: {hasSpawnedBranchesAt2}");
            }
            
            SpawnBranches();
            hasSpawnedBranchesAt2 = true;

            if (debugLogs)
                Debug.Log($"[TreeFallManager] hasSpawnedBranchesAt2 after: {hasSpawnedBranchesAt2}");
        }

        // Debug si on a manqué le spawn
        if (currentCrackCount >= 2 && debugLogs)
        {
            if (hasSpawnedBranchesAt2 && lastCrackCount < 2)
            {
                Debug.Log("[TreeFallManager] ⚠️ Spawn devrais être complété maintenant!");
            }
        }

        lastCrackCount = currentCrackCount;
    }

    /// <summary>
    /// Spawn plusieurs branches autour du barrage
    /// Mix de branches Poplar et Birch
    /// </summary>
    private void SpawnBranches()
    {
        if (debugLogs)
        {
            Debug.Log("═══════════════════════════════════════");
            Debug.Log("[TreeFallManager] SPAWN BRANCHES START");
            Debug.Log("═══════════════════════════════════════");
            Debug.Log($"[TreeFallManager] poplarBranchPrefab: {(poplarBranchPrefab != null ? "✓ ASSIGNED" : "✗ NULL")}");
            Debug.Log($"[TreeFallManager] birchBranchPrefab: {(birchBranchPrefab != null ? "✓ ASSIGNED" : "✗ NULL")}");
            Debug.Log($"[TreeFallManager] damManager: {(damManager != null ? "✓ ASSIGNED" : "✗ NULL")}");
        }

        if (poplarBranchPrefab == null || birchBranchPrefab == null)
        {
            Debug.LogError("[TreeFallManager] ✗✗✗ PREFABS NULL - SPAWN ABORTED ✗✗✗");
            return;
        }

        Vector2 spawnCenter = (Vector2)damManager.transform.position + spawnCenterOffset;
        int totalBranches = poplarBranchCount + birchBranchCount;

        if (debugLogs)
        {
            Debug.Log($"[TreeFallManager] Spawn center: {spawnCenter}");
            Debug.Log($"[TreeFallManager] Total branches to spawn: {totalBranches} ({poplarBranchCount} Poplar + {birchBranchCount} Birch)");
        }

        // Spawn Poplar branches
        for (int i = 0; i < poplarBranchCount; i++)
        {
            SpawnBranchOfType(poplarBranchPrefab, spawnCenter, i, "Poplar");
        }

        // Spawn Birch branches
        for (int i = 0; i < birchBranchCount; i++)
        {
            SpawnBranchOfType(birchBranchPrefab, spawnCenter, poplarBranchCount + i, "Birch");
        }

        if (debugLogs)
        {
            Debug.Log($"[TreeFallManager] ✓✓✓ {totalBranches} branches générées avec succès! ✓✓✓");
            Debug.Log("═══════════════════════════════════════");
        }
    }

    /// <summary>
    /// Spawne une branche d'un type spécifique
    /// </summary>
    private void SpawnBranchOfType(GameObject prefab, Vector2 spawnCenter, int index, string branchType)
    {
        // Position aléatoire autour du point de spawn
        Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPosition = new Vector3(
            spawnCenter.x + randomOffset.x,
            spawnCenter.y + randomOffset.y,
            0f
        );

        GameObject branchInstance = Instantiate(prefab, spawnPosition, Quaternion.identity, transform);
        branchInstance.name = $"{branchType}Branch_{index + 1}";

        // Vérifications de debug
        if (debugLogs)
        {
            BranchItem branchItem = branchInstance.GetComponent<BranchItem>();
            EquippableItem equippable = branchInstance.GetComponent<EquippableItem>();
            BranchRepairItem repairItem = branchInstance.GetComponent<BranchRepairItem>();
            
            Debug.Log($"  [{branchType.ToUpper()}] Branch #{index + 1} spawned");
            Debug.Log($"    └─ Position: {spawnPosition}");
            Debug.Log($"    ├─ BranchItem: {(branchItem != null ? $"✓ ({branchItem.GetTreeTypeName()})" : "✗ MISSING")}");
            Debug.Log($"    ├─ EquippableItem: {(equippable != null ? "✓" : "✗ MISSING")}");
            Debug.Log($"    └─ BranchRepairItem: {(repairItem != null ? "✓" : "✗ MISSING")}");

            // Vérifie que les composants correspondent au type
            if (branchItem != null)
            {
                bool typeMatches = (branchType == "Poplar" && branchItem.GetTreeType() == TreeType.Poplar) ||
                                   (branchType == "Birch" && branchItem.GetTreeType() == TreeType.Birch);
                Debug.Log($"    └─ Type match: {(typeMatches ? "✓ CORRECT" : "✗ MISMATCH!")}");
            }
        }
    }

    /// <summary>
    /// Reset pour les tests en éditeur
    /// </summary>
    public void ResetSpawnState()
    {
        hasSpawnedBranchesAt2 = false;
        lastCrackCount = 0;
    }
}
