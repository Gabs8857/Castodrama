using UnityEngine;

/// <summary>
/// Represents a food item that restores hunger when picked up by the player.
/// Automatically configures required components (Collider2D, SpriteRenderer, Rigidbody2D).
/// </summary>
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
public class FoodItem : MonoBehaviour
{
    private const int MinVisibleSortingOrder = 200;
    private static Sprite defaultVisualSprite;

    [SerializeField]
    private float hungerRestoreAmount = 20f;

    [SerializeField]
    private Sprite foodSprite;

    private void Awake()
    {
        SetupVisuals();
        SetupPhysics();
    }

    private void SetupVisuals()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Use provided sprite, or create a default yellow square
        Sprite spriteToDisplay = foodSprite ?? GetDefaultVisualSprite();
        
        spriteRenderer.sprite = spriteToDisplay;
        spriteRenderer.color = Color.white;
        
        // Ensure visibility (high sorting order)
        if (spriteRenderer.sortingOrder < MinVisibleSortingOrder)
        {
            spriteRenderer.sortingOrder = MinVisibleSortingOrder;
        }
    }

    private void SetupPhysics()
    {
        // Configure collider as trigger
        Collider2D collider = GetComponent<Collider2D>();
        collider.isTrigger = true;

        // Configure rigidbody for trigger interactions
        Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();
        rigidbody.bodyType = RigidbodyType2D.Kinematic;
        rigidbody.gravityScale = 0f;
        rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        rigidbody.simulated = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Try to feed the player
        if (TryFeedPlayer(collision))
        {
            Destroy(gameObject);
        }
    }

    private bool TryFeedPlayer(Collider2D collision)
    {
        // Feed the player using TopDownHunger system
        TopDownHunger hungerSystem = collision.GetComponentInParent<TopDownHunger>();
        if (hungerSystem != null)
        {
            hungerSystem.AddHunger(hungerRestoreAmount);
            return true;
        }

        return false;
    }

    private static Sprite GetDefaultVisualSprite()
    {
        if (defaultVisualSprite != null)
        {
            return defaultVisualSprite;
        }

        // Create a visible 32x32 yellow square as placeholder
        Texture2D texture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[32 * 32];
        
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.yellow;
        }
        
        texture.SetPixels(pixels);
        texture.Apply();

        defaultVisualSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, 32f, 32f),
            new Vector2(0.5f, 0.5f),
            32f
        );

        return defaultVisualSprite;
    }
}