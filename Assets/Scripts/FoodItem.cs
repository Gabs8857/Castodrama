using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public class FoodItem : MonoBehaviour
{
    private static Sprite fallbackSprite;
    private const int MinVisibleSortingOrder = 200;

    [SerializeField]
    private float hungerAmount = 20f;

    [SerializeField]
    private Sprite pickupSprite;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Sprite spriteToUse = pickupSprite;
            if (!IsValidProjectSprite(spriteToUse))
            {
                spriteToUse = GetFallbackSprite();
            }

            spriteRenderer.sprite = spriteToUse;
            spriteRenderer.enabled = true;
            spriteRenderer.color = Color.white;
            if (spriteRenderer.sortingOrder < MinVisibleSortingOrder)
            {
                spriteRenderer.sortingOrder = MinVisibleSortingOrder;
            }
        }

        Collider2D pickupCollider = GetComponent<Collider2D>();
        pickupCollider.isTrigger = true;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.simulated = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TopDownHunger topDownHunger = other.GetComponentInParent<TopDownHunger>();
        if (topDownHunger != null)
        {
            topDownHunger.AddHunger(hungerAmount);
            Destroy(gameObject);
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        Hunger legacyHunger = Object.FindFirstObjectByType<Hunger>();
        if (legacyHunger == null)
        {
            return;
        }

        legacyHunger.currentHunger = Mathf.Clamp(legacyHunger.currentHunger + hungerAmount, 0f, legacyHunger.maxHunger);
        Destroy(gameObject);
    }

    private static Sprite GetFallbackSprite()
    {
        if (fallbackSprite != null)
        {
            return fallbackSprite;
        }

        fallbackSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        return fallbackSprite;
    }

    private static bool IsValidProjectSprite(Sprite sprite)
    {
        if (sprite == null)
        {
            return false;
        }

        string path = sprite.texture != null ? sprite.texture.name : string.Empty;
        return !string.IsNullOrEmpty(path);
    }
}