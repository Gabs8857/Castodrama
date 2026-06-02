using UnityEngine;

/// <summary>
/// Branche de Bouleau (Birch)
/// Peut être utilisée pour réparer le barrage
/// </summary>
public class BirchBranch : BranchItem
{
    protected override void Awake()
    {
        treeType = TreeType.Birch;
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        if (debugLogs)
            Debug.Log($"[BirchBranch] ✓ Birch branch initialized on {gameObject.name}");
    }
}
