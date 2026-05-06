using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

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
    private Sprite itemSprite;

    private TopDownPlayerController player;
    private bool isPickedUp = false;
    private float pickupDelay = 0.5f;
    private float timeSinceSpawn = 0f;
    
    private Canvas grabMessageCanvas;
    private TextMeshProUGUI grabMessageText;

    private void Awake()
    {
        // Configure le SpriteRenderer
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && itemSprite != null)
        {
            spriteRenderer.sprite = itemSprite;
            spriteRenderer.sortingOrder = 50;
        }

        // Configure collider en trigger
        Collider2D collider = GetComponent<Collider2D>();
        collider.isTrigger = true;

        // Configure rigidbody comme FoodItem
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.simulated = true;
        
        Debug.Log($"[EquippableItem] Setup complet pour {gameObject.name}");
    }

    private void Update()
    {
        timeSinceSpawn += Time.deltaTime;

        if (player != null && !isPickedUp && timeSinceSpawn >= pickupDelay)
        {
            if (Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame)
            {
                Debug.Log($"[EquippableItem] G pressé!");
                if (player.PickUpItem(gameObject))
                {
                    OnItemPickedUp();
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"[EquippableItem] OnTriggerEnter2D!!! Collision avec: {collision.gameObject.name}");
        
        if (isPickedUp)
            return;

        if (timeSinceSpawn < pickupDelay)
            return;

        player = collision.GetComponentInParent<TopDownPlayerController>();
        if (player == null)
            player = collision.GetComponent<TopDownPlayerController>();

        if (player != null)
        {
            Debug.Log($"[EquippableItem] ✓ Joueur trouvé!");
            ShowGrabMessage();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        TopDownPlayerController controller = collision.GetComponentInParent<TopDownPlayerController>();
        if (controller == null)
            controller = collision.GetComponent<TopDownPlayerController>();
        
        if (controller == player)
        {
            Debug.Log($"[EquippableItem] Joueur sort");
            HideGrabMessage();
            player = null;
        }
    }

    private void OnItemPickedUp()
    {
        isPickedUp = true;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0;
        }

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 100;
        }

        HideGrabMessage();

        if (destroyOnPickup)
        {
            Destroy(gameObject);
        }
    }

    public void Drop()
    {
        if (player != null)
        {
            player.DropItem();
        }

        isPickedUp = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = true;
        }

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 50;
        }
    }

    private void ShowGrabMessage()
    {
        if (grabMessageCanvas == null)
        {
            CreateGrabMessage();
        }

        if (grabMessageCanvas != null)
        {
            grabMessageCanvas.gameObject.SetActive(true);
        }
    }

    private void HideGrabMessage()
    {
        if (grabMessageCanvas != null)
        {
            grabMessageCanvas.gameObject.SetActive(false);
        }
    }

    private void CreateGrabMessage()
    {
        GameObject canvasGO = new GameObject("GrabMessage");
        canvasGO.transform.SetParent(transform);
        canvasGO.transform.localPosition = Vector3.zero;

        grabMessageCanvas = canvasGO.AddComponent<Canvas>();
        grabMessageCanvas.renderMode = RenderMode.WorldSpace;

        RectTransform canvasRect = canvasGO.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(120, 50);
        canvasRect.localPosition = new Vector3(0, 1.5f, 0);
    }
}
