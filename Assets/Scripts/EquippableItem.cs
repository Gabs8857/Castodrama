using UnityEngine;

/// <summary>
/// Script pour les items qui peuvent être ramassés et équippés par le joueur.
/// L'item suivra le joueur après avoir été ramassé.
/// À attacher sur un GameObject avec un BoxCollider2D/CircleCollider2D en Trigger.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class EquippableItem : MonoBehaviour
{
    [SerializeField]
    private bool destroyOnPickup = false; // Si true, détruit l'item après pickup; si false, le garde pour le suivre

    [SerializeField]
    private string itemName = "Item";

    private TopDownPlayerController player;
    private bool isPickedUp = false;

    private void Awake()
    {
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
        else
        {
            // Ajoute un rigidbody s'il n'en a pas
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 1f;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isPickedUp)
            return;

        // Cherche le TopDownPlayerController
        player = collision.GetComponent<TopDownPlayerController>();
        
        if (player == null)
        {
            // Essaie de le trouver dans le parent
            player = collision.GetComponentInParent<TopDownPlayerController>();
        }

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

        // Désactive la gravité et configure le rigidbody si présent
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0;
        }

        // Désactive le collider pour éviter les interactions
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // Augmente le sorting order pour que l'item soit visible devant le joueur
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 100;
        }

        Debug.Log($"Item ramassé: {itemName}");

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

        // Réactive les composants
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 1;
        }

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

        Debug.Log($"Item lâché: {itemName}");
    }
}
