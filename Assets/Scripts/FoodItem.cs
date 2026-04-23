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
    private static Material unlitSpriteMaterial;

    [SerializeField]
    private float hungerRestoreAmount = 20f;

    [SerializeField]
    private Sprite foodSprite;

    [SerializeField]
    private bool useUnlitMaterial = true;

    private void Awake()
    {
        SetupVisuals();
        SetupPhysics();
    }

    private void SetupVisuals()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Use provided sprite, or create a default brown circular icon
        Sprite spriteToDisplay = foodSprite ?? GetDefaultVisualSprite();
        
        spriteRenderer.sprite = spriteToDisplay;
        spriteRenderer.color = Color.white;

        if (useUnlitMaterial)
        {
            Material material = GetUnlitSpriteMaterial();
            if (material != null)
            {
                spriteRenderer.sharedMaterial = material;
            }
        }
        
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

        // Create a visible 32x32 brown circle as placeholder (beaver-themed)
        Texture2D texture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[32 * 32];

        Color outerBrown = new Color(0.43f, 0.26f, 0.13f, 1f);
        Color innerBrown = new Color(0.56f, 0.34f, 0.18f, 1f);
        float center = 15.5f;
        float radius = 14f;
        float innerRadius = 11.5f;

        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                int index = y * 32 + x;

                if (dist <= innerRadius)
                {
                    pixels[index] = innerBrown;
                }
                else if (dist <= radius)
                {
                    pixels[index] = outerBrown;
                }
                else
                {
                    pixels[index] = new Color(0f, 0f, 0f, 0f);
                }
            }
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

    private static Material GetUnlitSpriteMaterial()
    {
        if (unlitSpriteMaterial != null)
        {
            return unlitSpriteMaterial;
        }

        Shader unlitShader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (unlitShader == null)
        {
            return null;
        }

        unlitSpriteMaterial = new Material(unlitShader);
        return unlitSpriteMaterial;
    }
}