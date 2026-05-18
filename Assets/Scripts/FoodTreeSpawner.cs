using UnityEngine;

/// <summary>
/// Spawns edible food trees at predefined positions on the map.
/// Each tree is an instance of the FoodTree prefab with FoodItem component.
/// </summary>
public class FoodTreeSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject foodTreePrefab;

    [SerializeField]
    private Vector2[] treeSpawnPositions = new Vector2[]
    {
        new Vector2(-5f, 3f),
        new Vector2(-2f, 5f),
        new Vector2(0f, 2f),
        new Vector2(3f, 4f),
        new Vector2(6f, 1f),
        new Vector2(-8f, -1f),
        new Vector2(-4f, -4f),
        new Vector2(1f, -3f),
        new Vector2(5f, -2f),
        new Vector2(8f, -5f),
        new Vector2(-6f, 6f),
        new Vector2(4f, 6f),
        new Vector2(-1f, -6f),
        new Vector2(7f, 3f),
        new Vector2(2f, 7f),
    };

    private void Start()
    {
        if (foodTreePrefab == null)
        {
            Debug.LogError("FoodTreeSpawner: foodTreePrefab is not assigned!");
            return;
        }

        SpawnTrees();
    }

    private void SpawnTrees()
    {
        foreach (Vector2 position in treeSpawnPositions)
        {
            GameObject treeInstance = Instantiate(
                foodTreePrefab,
                new Vector3(position.x, position.y, 0f),
                Quaternion.identity,
                transform
            );
            treeInstance.name = $"FoodTree_{treeInstance.GetInstanceID()}";
        }

        Debug.Log($"Spawned {treeSpawnPositions.Length} food trees");
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Visualize spawn positions in the editor
        Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.7f);
        foreach (Vector2 position in treeSpawnPositions)
        {
            Gizmos.DrawWireSphere(new Vector3(position.x, position.y, 0f), 0.3f);
        }
    }
#endif
}
