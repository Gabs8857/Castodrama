using UnityEngine;

/// <summary>
/// Branche de Peuplier (Poplar)
/// Peut être utilisée pour réparer le barrage
/// </summary>
public class PoplarBranch : BranchItem
{
    protected override void Awake()
    {
        treeType = TreeType.Poplar;
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        if (debugLogs)
            Debug.Log($"[PoplarBranch] ✓ Poplar branch initialized on {gameObject.name}");
    }
}
