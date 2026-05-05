using UnityEngine;

/// <summary>
/// Script pour les items qui peuvent être ramassés et équippés par le joueur.
/// L'item suivra le joueur après avoir été ramassé.
/// À attacher sur un GameObject avec un BoxCollider2D/CircleCollider2D en Trigger.
/// </summary>
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
public class EquippableItem : MonoBehaviour
{
    [SerializeField]
    private bool destroyOnPickup = false;

    [SerializeField]
    private string itemName = "Item";

    [SerializeField]
    private Sprite itemSprite; // Assigne ton sprite ici dans l'Inspector

    private TopDownPlayerController player;
    private bool isPickedUp = false;
    private float pickupDelay = 0.5f; // Délai avant de pouvoir être ramassé
    private float timeSinceSpawn = 0f;

    private void Awake()
    {
        // Configure le SpriteRenderer
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && itemSprite != null)
        {
            spriteRenderer.sprite = itemSprite;
        }

        // Force le collider en trigger
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }

        // Configure le rigidbody
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 1f;
            rb.simulated = true;
        }
    }

    private void Update()
    {
        timeSinceSpawn += Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isPickedUp)
            return;

        // Empêche de ramasser l'item immédiatement au démarrage
        if (timeSinceSpawn < pickupDelay)
            return;

        // Cherche le TopDownPlayerController
        player = collision.GetComponent<TopDownPlayerController>();
        
        if (player == null)
            player = collision.GetComponentInParent<TopDownPlayerController>();
        
        if (player == null)
            player = collision.GetComponentInChildren<TopDownPlayerController>();
        
        if (player == null)
            player = FindObjectOfType<TopDownPlayerController>();

        if (player != null)
        {
            if (player.PickUpItem(gameObject))
            {
                OnItemPickedUp();
            }
        }
    }

    private void OnItemPickedUp()
    {
        isPickedUp = true;

        // Désactive la gravité et configure le rigidbody
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0;
        }

        // Désactive le collider
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // Augmente le sorting order pour visibilité
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 100;
        }

        if (destroyOnPickup)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Appelle cette méthode pour lâcher l'item (ex: au appui sur une touche).
    /// </summary>
    public void Drop()
    {
        if (player != null)
        {
            player.DropItem();
        }

        isPickedUp = false;

        // Réactive le rigidbody
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 1;
        }

        // Réactive le collider
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = true;
        }

        // Réinitialise le sorting order
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 0;
        }
    }
}
