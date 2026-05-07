using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Contrôleur principal du joueur (Castor)
/// Gère:
/// - Le mouvement top-down (clavier + gamepad)
/// - L'inventaire (ramassage et dépôt d'items)
/// - La détection centralisée des zones (eau, lave, feu, etc.)
/// - La notification des autres systèmes (animations, items, etc.)
/// 
/// C'est le centre névralgique auquel tous les autres scripts se connectent.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class TopDownPlayerController : MonoBehaviour, IZoneDetectable
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

    // Détection de zones - centralisée
    private ZoneDetectionManager zoneManager;

    public float MoveSpeed
    {
        get => moveSpeed;
        set => moveSpeed = Mathf.Max(0f, value);
    }

    public bool HasItem => hasItem;
    public GameObject EquippedItem => equippedItem;
    
    /// <summary>
    /// Accès au gestionnaire de zones
    /// </summary>
    public ZoneDetectionManager ZoneManager => zoneManager;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        
        // Initialise le gestionnaire de zones
        zoneManager = new ZoneDetectionManager(this);
        
        Debug.Log($"[TopDownPlayerController] Initialisation complète");
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
                spriteRenderer.flipX = false;  // Gauche = pas flipper
            }
            else if (moveInput.x > inputDeadzone)
            {
                spriteRenderer.flipX = true;   // Droite = flipper
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
        // L'item équippé suit le joueur avec offset adapté selon la direction
        if (equippedItem != null)
        {
            Vector3 adjustedOffset = itemOffset;
            if (spriteRenderer.flipX)
            {
                adjustedOffset.x = -itemOffset.x;
            }
            equippedItem.transform.localPosition = adjustedOffset;
        }
    }

    /// <summary>
    /// Tente de ramasser un item. Retourne true si succès, false si le joueur a déjà un item.
    /// </summary>
    public bool PickUpItem(GameObject item)
    {
        Debug.Log($"[TopDownPlayerController] PickUpItem appelé pour: {item.name}, hasItem={hasItem}");
        
        if (hasItem)
        {
            Debug.LogWarning($"[TopDownPlayerController] Grab refusé - joueur a déjà un item!");
            return false;
        }

        Debug.Log($"[TopDownPlayerController] ✓ Grab de {item.name} accepté!");
        
        equippedItem = item;
        hasItem = true;
        
        // Parente l'item au joueur
        item.transform.SetParent(transform);
        item.transform.localPosition = itemOffset;
        Debug.Log($"[TopDownPlayerController] Item parenté et positionné");
        
        // Démarre l'animation
        CharacterAnimator playerAnimator = GetComponent<CharacterAnimator>();
        if (playerAnimator != null)
        {
            playerAnimator.StartSwimming();
            Debug.Log($"[TopDownPlayerController] Animation nage démarrée");
        }
        
        return true;
    }

    /// <summary>
    /// Dépose l'item actuellement équippé.
    /// </summary>
    public void DropItem()
    {
        if (equippedItem != null)
        {
            // Arrête l'animation
            CharacterAnimator playerAnimator = GetComponent<CharacterAnimator>();
            if (playerAnimator != null)
            {
                playerAnimator.StopSwimming();
            }
            
            // Détache l'item du joueur
            equippedItem.transform.SetParent(null);
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

    /// <summary>
    /// Implémentation de IZoneDetectable - Appelé quand le joueur entre dans une zone
    /// </summary>
    public void OnEnterZone(ZoneType zoneType)
    {
        Debug.Log($"[TopDownPlayerController] ✓ ENTRÉE zone: {zoneType}");
        zoneManager?.EnterZone(zoneType);
        
        // Notifie les autres composants (CharacterAnimator, EquippableItem, etc.)
        IZoneDetectable[] detectables = GetComponents<IZoneDetectable>();
        foreach (IZoneDetectable detectable in detectables)
        {
            if (detectable != this) // Évite de se notifier soi-même
            {
                detectable.OnEnterZone(zoneType);
            }
        }
    }

    /// <summary>
    /// Implémentation de IZoneDetectable - Appelé quand le joueur sort d'une zone
    /// </summary>
    public void OnExitZone(ZoneType zoneType)
    {
        Debug.Log($"[TopDownPlayerController] ✓ SORTIE zone: {zoneType}");
        zoneManager?.ExitZone(zoneType);
        
        // Notifie les autres composants
        IZoneDetectable[] detectables = GetComponents<IZoneDetectable>();
        foreach (IZoneDetectable detectable in detectables)
        {
            if (detectable != this) // Évite de se notifier soi-même
            {
                detectable.OnExitZone(zoneType);
            }
        }
    }
}
