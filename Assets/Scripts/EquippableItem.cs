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
        Debug.Log($"[EquippableItem.Awake] Initialisation de {itemName}");

        // Configure le SpriteRenderer
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && itemSprite != null)
        {
            spriteRenderer.sprite = itemSprite;
            Debug.Log($"[EquippableItem.Awake] Sprite assigné");
        }
        else if (spriteRenderer == null)
        {
            Debug.LogError($"[EquippableItem.Awake] ERREUR: Pas de SpriteRenderer trouvé!");
        }

        // Force le collider en trigger
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
            Debug.Log($"[EquippableItem.Awake] Collider configuré en trigger");
        }
        else
        {
            Debug.LogError($"[EquippableItem.Awake] ERREUR: Pas de Collider2D trouvé!");
        }

        // Configure le rigidbody
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 1f;
            rb.simulated = true;
            Debug.Log($"[EquippableItem.Awake] Rigidbody configuré");
        }
        else
        {
            Debug.LogError($"[EquippableItem.Awake] ERREUR: Pas de Rigidbody2D trouvé!");
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        Debug.Log($"[EquippableItem.OnTriggerStay2D] En contact continu avec: {collision.gameObject.name}");
    }

    private void Update()
    {
        timeSinceSpawn += Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"[EquippableItem.OnTriggerEnter2D] DÉBUT - Collision detectée avec: {collision.gameObject.name}");
        Debug.Log($"[EquippableItem.OnTriggerEnter2D] isPickedUp: {isPickedUp}, timeSinceSpawn: {timeSinceSpawn}");

        if (isPickedUp)
        {
            Debug.Log($"[EquippableItem.OnTriggerEnter2D] Item déjà ramassé, sortie");
            return;
        }

        // Empêche de ramasser l'item immédiatement au démarrage
        if (timeSinceSpawn < pickupDelay)
        {
            Debug.Log($"[EquippableItem.OnTriggerEnter2D] Délai de sécurité pas encore écoulé ({timeSinceSpawn:F2}s / {pickupDelay}s)");
            return;
        }

        Debug.Log($"[EquippableItem.OnTriggerEnter2D] Collision avec: {collision.gameObject.name} (Tag: {collision.gameObject.tag})");

        // Cherche le TopDownPlayerController directement
        player = collision.GetComponent<TopDownPlayerController>();
        Debug.Log($"[EquippableItem.OnTriggerEnter2D] GetComponent<TopDownPlayerController>: {(player != null ? "TROUVÉ" : "NON")}");
        
        if (player == null)
        {
            player = collision.GetComponentInParent<TopDownPlayerController>();
            Debug.Log($"[EquippableItem.OnTriggerEnter2D] GetComponentInParent<TopDownPlayerController>: {(player != null ? "TROUVÉ" : "NON")}");
        }

        if (player == null)
        {
            player = collision.GetComponentInChildren<TopDownPlayerController>();
            Debug.Log($"[EquippableItem.OnTriggerEnter2D] GetComponentInChildren<TopDownPlayerController>: {(player != null ? "TROUVÉ" : "NON")}");
        }

        if (player == null)
        {
            player = FindObjectOfType<TopDownPlayerController>();
            Debug.Log($"[EquippableItem.OnTriggerEnter2D] FindObjectOfType<TopDownPlayerController>: {(player != null ? "TROUVÉ" : "NON")}");
        }

        if (player != null)
        {
            Debug.Log($"[EquippableItem.OnTriggerEnter2D] ✓ TopDownPlayerController TROUVÉ! Tentative PickUpItem...");
            bool pickupSuccess = player.PickUpItem(gameObject);
            Debug.Log($"[EquippableItem.OnTriggerEnter2D] PickUpItem retourné: {pickupSuccess}");
            
            if (pickupSuccess)
            {
                OnItemPickedUp();
            }
            else
            {
                Debug.LogWarning("[EquippableItem.OnTriggerEnter2D] Le joueur a déjà un item!");
            }
        }
        else
        {
            Debug.LogError($"[EquippableItem.OnTriggerEnter2D] ✗ ERREUR: Impossible de trouver TopDownPlayerController!");
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
