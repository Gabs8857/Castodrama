using UnityEngine;

/// <summary>
/// Bouleau (Birch) - Comestible food item with progression system.
/// First eat: frame 2 -> Second eat: frame 3 -> Third eat: disappears
/// Also spawns branches when tree is eaten (transitions to frame 2)
/// </summary>
public class BirchFoodItem : FoodItem, IBranchSpawner
{
    [SerializeField]
    private GameObject birchBranchPrefab;

    [SerializeField]
    private bool debugLogs = true;

    protected override void Awake()
    {
        // Set Birch-specific defaults
        hungerRestoreAmount = 15f;
        visibleTint = new Color(0.8f, 0.7f, 0.5f, 1f); // Birch light color
        glowIntensity = 0.8f;
        base.Awake();
    }

    /// <summary>
    /// Override TryFeedPlayer pour spawner les branches quand on mange l'arbre
    /// </summary>
    protected override void TryFeedPlayer(Collider2D collision)
    {
        TopDownHunger hungerSystem = collision.GetComponentInParent<TopDownHunger>();
        if (hungerSystem != null)
        {
            hungerSystem.AddHunger(hungerRestoreAmount);

            if (eatCount < 2)
            {
                if (eatProgressionSprites != null && eatCount < eatProgressionSprites.Length && eatProgressionSprites[eatCount] != null)
                {
                    SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
                    spriteRenderer.sprite = eatProgressionSprites[eatCount];
                }
                eatCount++;

                // Spawne les branches quand on passe en sprite 2 (eatCount == 1)
                if (eatCount == 1)
                {
                    SpawnBranches(1, 1.5f);
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// Spawne des branches de bouleau autour de cet arbre
    /// Implémentation de IBranchSpawner
    /// </summary>
    public void SpawnBranches(int count = 3, float radius = 2f)
    {
        if (birchBranchPrefab == null)
        {
            if (debugLogs)
                Debug.LogWarning($"[BirchFoodItem] No birch branch prefab assigned for {gameObject.name}!");
            return;
        }

        if (debugLogs)
            Debug.Log($"[BirchFoodItem] ✓ Spawning {count} Birch branches around {gameObject.name}");

        for (int i = 0; i < count; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * radius;
            Vector3 spawnPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

            GameObject branchInstance = Instantiate(birchBranchPrefab, spawnPosition, Quaternion.identity);
            branchInstance.name = $"BirchBranch_{i + 1}";

            if (debugLogs)
                Debug.Log($"[BirchFoodItem] Branch #{i + 1} spawned at {spawnPosition}");
        }
    }
}
