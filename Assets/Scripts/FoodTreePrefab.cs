using UnityEngine;

/// <summary>
/// Setup script for food tree prefab instances.
/// Ensures the tree has all required components for the FoodItem system.
/// This script runs once on Awake to configure the tree.
/// </summary>
public class FoodTreePrefab : MonoBehaviour
{
    [SerializeField]
    private Sprite treeSprite;

    [SerializeField]
    private float hungerRestoreAmount = 25f;

    private void Awake()
    {
        EnsureComponents();
    }

    private void EnsureComponents()
    {
        // Ensure SpriteRenderer exists
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        if (treeSprite != null)
        {
            spriteRenderer.sprite = treeSprite;
        }
        spriteRenderer.sortingOrder = 100;

        // Ensure Collider2D exists and is a trigger
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider2D>();
        }
        collider.isTrigger = true;

        // Ensure Rigidbody2D exists and is kinematic
        Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();
        if (rigidbody == null)
        {
            rigidbody = gameObject.AddComponent<Rigidbody2D>();
        }
        rigidbody.bodyType = RigidbodyType2D.Kinematic;
        rigidbody.gravityScale = 0f;
        rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        rigidbody.simulated = true;

        // Ensure FoodItem exists and has correct values
        FoodItem foodItem = GetComponent<FoodItem>();
        if (foodItem == null)
        {
            foodItem = gameObject.AddComponent<FoodItem>();
        }
    }
}
