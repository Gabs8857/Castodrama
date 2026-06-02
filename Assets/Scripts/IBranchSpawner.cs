using UnityEngine;

/// <summary>
/// Interface pour les arbres qui peuvent spawner des branches
/// </summary>
public interface IBranchSpawner
{
    /// <summary>
    /// Spawne des branches à proximité de l'arbre
    /// </summary>
    void SpawnBranches(int count = 3, float radius = 2f);
}
