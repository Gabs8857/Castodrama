using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
public class TopDownPlayerController : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 6.5f;

    [SerializeField]
    private float inputDeadzone = 0.12f;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Vector2 moveInput;

    // Inventaire
    private GameObject equippedItem;
    private bool hasItem = false;
    private Vector3 itemOffset = new Vector3(0.3f, 0.2f, 0); // Position relative par rapport au joueur

    public float MoveSpeed
    {
        get => moveSpeed;
        set => moveSpeed = Mathf.Max(0f, value);
    }

    public bool HasItem => hasItem;
    public GameObject EquippedItem => equippedItem;

    private void Awake()
    {
        Debug.Log("[TopDownPlayerController] Awake() appelé");
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        Debug.Log($"[TopDownPlayerController] SpriteRenderer trouvé: {(spriteRenderer != null ? "OUI" : "NON")}");
        Debug.Log($"[TopDownPlayerController] Rigidbody2D trouvé: {(rb != null ? "OUI" : "NON")}");
    }

    private void Update()
    {
        moveInput = ReadMoveInput();
        if (moveInput.sqrMagnitude > 1f)
        {
            moveInput.Normalize();
        }

        if (spriteRenderer != null)
        {
            if (moveInput.x < -inputDeadzone)
            {
                spriteRenderer.flipX = true;
            }
            else if (moveInput.x > inputDeadzone)
            {
                spriteRenderer.flipX = false;
            }
        }
    }

    private void FixedUpdate()
    {
        if (rb != null)
        {
            rb.linearVelocity = moveInput * moveSpeed;
        }
    }

    private void LateUpdate()
    {
        // L'item équippé suit le joueur
        if (equippedItem != null)
        {
            // Adapter l'offset selon la direction du personnage
            Vector3 adjustedOffset = itemOffset;
            if (spriteRenderer.flipX)
            {
                adjustedOffset.x = -itemOffset.x;
            }

            equippedItem.transform.position = transform.position + adjustedOffset;
        }
    }

    /// <summary>
    /// Tente de ramasser un item. Retourne true si succès, false si le joueur a déjà un item.
    /// </summary>
    public bool PickUpItem(GameObject item)
    {
        Debug.Log($"[TopDownPlayerController.PickUpItem] Appelé avec: {item.name}");
        Debug.Log($"[TopDownPlayerController.PickUpItem] hasItem: {hasItem}");
        
        if (hasItem)
        {
            Debug.Log($"[TopDownPlayerController.PickUpItem] Déjà un item, retour false");
            return false;
        }

        equippedItem = item;
        hasItem = true;
        Debug.Log($"[TopDownPlayerController.PickUpItem] Item ramassé avec succès!");
        return true;
    }

    /// <summary>
    /// Dépose l'item actuellement équippé.
    /// </summary>
    public void DropItem()
    {
        if (equippedItem != null)
        {
            equippedItem = null;
            hasItem = false;
        }
    }

    private Vector2 ReadMoveInput()
    {
        Vector2 keyboardInput = Vector2.zero;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            {
                keyboardInput.x -= 1f;
            }

            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            {
                keyboardInput.x += 1f;
            }

            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            {
                keyboardInput.y -= 1f;
            }

            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            {
                keyboardInput.y += 1f;
            }
        }

        if (keyboardInput.sqrMagnitude > 1f)
        {
            keyboardInput.Normalize();
        }

        Vector2 gamepadInput = Gamepad.current != null ? Gamepad.current.leftStick.ReadValue() : Vector2.zero;
        if (gamepadInput.magnitude < inputDeadzone)
        {
            gamepadInput = Vector2.zero;
        }

        return gamepadInput.sqrMagnitude > keyboardInput.sqrMagnitude ? gamepadInput : keyboardInput;
    }
}
