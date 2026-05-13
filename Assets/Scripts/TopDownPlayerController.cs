using UnityEngine;

/// <summary>
/// Contrôleur du joueur principal (Castor) - Classe spécialisée
/// Hérité de PlayerController qui gère le mouvement de base
/// 
/// Ajoute au mouvement:
/// - L'inventaire (ramassage et dépôt d'items)
/// - La détection centralisée des zones (eau, lave, feu, etc.)
/// - La notification des autres systèmes (animations, items, etc.)
/// - La gestion du sprite selon la direction
/// 
/// C'est le centre névralgique auquel tous les autres scripts se connectent.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class TopDownPlayerController : PlayerController, IZoneDetectable
{
    private SpriteRenderer spriteRenderer;

    // Inventaire
    private GameObject equippedItem;
    private bool hasItem = false;
    
    [SerializeField]
    private Vector3 itemOffsetRight = new Vector3(-0.33f, 0.03f, 0); // Position quand regarde à droite
    
    [SerializeField]
    private Vector3 itemOffsetLeft = new Vector3(0.32f, 0.03f, 0); // Position quand regarde à gauche
    
    [SerializeField]
    private Vector3 itemOffsetUp = new Vector3(0.0f, 1f, 0); // Position quand regarde vers le haut
    
    [SerializeField]
    private Vector3 itemOffsetDown = new Vector3(-0.2f, -0.7f, 0); // Position quand regarde vers le bas
    
    [SerializeField]
    private float itemRotationRight = 142.7f; // Rotation Z quand regarde à droite
    
    [SerializeField]
    private float itemRotationLeft = 936.2f; // Rotation Z quand regarde à gauche
    
    [SerializeField]
    private float itemRotationUp = -114.6f; // Rotation Z quand regarde vers le haut
    
    [SerializeField]
    private float itemRotationDown = 86.2f; // Rotation Z quand regarde vers le bas
    
    private int defaultSortOrder = 0; // Stocke le sort order initial du personnage
    private Vector2 lastMovementDirection = Vector2.down; // Dernière direction mémorisée

    // Détection de zones - centralisée
    private ZoneDetectionManager zoneManager;

    public bool HasItem => hasItem;
    public GameObject EquippedItem => equippedItem;
    
    /// <summary>
    /// Accès au gestionnaire de zones
    /// </summary>
    public ZoneDetectionManager ZoneManager => zoneManager;

    protected override void Awake()
    {
        base.Awake(); // Appelle Awake du PlayerController
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Stocke le sort order initial
        if (spriteRenderer != null)
        {
            defaultSortOrder = spriteRenderer.sortingOrder;
        }
        
        // Initialise le gestionnaire de zones
        zoneManager = new ZoneDetectionManager(this);
        
        Debug.Log($"[TopDownPlayerController] Initialisation complète");
    }

    protected override void Update()
    {
        base.Update(); // Appelle Update du PlayerController (lecture des entrées)
        
        // Mémorise la dernière direction de mouvement
        if (moveInput.sqrMagnitude > 0.01f)
        {
            lastMovementDirection = moveInput.normalized;
        }
        
        // Ajoute la gestion du sprite selon la direction
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

    private void LateUpdate()
    {
        // L'item équippé suit le joueur avec offset adapté selon la direction
        if (equippedItem != null)
        {
            // Utilise la dernière direction mémorisée (même quand immobile)
            Vector3 adjustedOffset;
            float rotation;
            
            float absX = Mathf.Abs(lastMovementDirection.x);
            float absY = Mathf.Abs(lastMovementDirection.y);
            
            if (absY > absX)
            {
                // Mouvement principalement vertical
                if (lastMovementDirection.y > 0)
                {
                    // Vers le haut
                    adjustedOffset = itemOffsetUp;
                    rotation = itemRotationUp;
                    // Change le sort order du personnage quand walk up
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.sortingOrder = 22;
                    }
                }
                else
                {
                    // Vers le bas
                    adjustedOffset = itemOffsetDown;
                    rotation = itemRotationDown;
                    // Restaure le sort order original
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.sortingOrder = defaultSortOrder;
                    }
                }
            }
            else
            {
                // Mouvement horizontal
                if (spriteRenderer != null)
                {
                    spriteRenderer.sortingOrder = defaultSortOrder;
                }
                
                if (spriteRenderer.flipX)
                {
                    // Vers la droite
                    adjustedOffset = itemOffsetRight;
                    rotation = itemRotationRight;
                }
                else
                {
                    // Vers la gauche
                    adjustedOffset = itemOffsetLeft;
                    rotation = itemRotationLeft;
                }
            }
            
            equippedItem.transform.localPosition = adjustedOffset;
            
            // Applique la rotation Z
            Vector3 eulerAngles = equippedItem.transform.localEulerAngles;
            eulerAngles.z = rotation;
            equippedItem.transform.localEulerAngles = eulerAngles;

            // Faire miroir le sprite de l'item selon la direction du joueur
            SpriteRenderer itemSpriteRenderer = equippedItem.GetComponent<SpriteRenderer>();
            if (itemSpriteRenderer != null)
            {
                itemSpriteRenderer.flipX = spriteRenderer.flipX;
            }
        }
    }

    /// <summary>
    /// Tente de ramasser un item. Retourne true si succès, false si le joueur a déjà un item.
    /// IMPORTANT: Conserve l'état d'animation actuel du personnage (nage/marche)
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
        
        // Log l'état actuel du personnage avant de ramasser
        CharacterAnimator playerAnimator = GetComponent<CharacterAnimator>();
        if (playerAnimator != null)
        {
            bool wasSwimming = playerAnimator.IsSwimming || playerAnimator.IsSwimmingDeep;
            Debug.Log($"[TopDownPlayerController] État du personnage: {(wasSwimming ? "NAGE" : "MARCHE")} - État CONSERVÉ après pickup");
        }
        
        equippedItem = item;
        hasItem = true;
        
        // Parente l'item au joueur
        item.transform.SetParent(transform);
        
        // Détermine la direction actuelle et applique l'offset/rotation approprié
        Vector3 currentOffset;
        float currentRotation;
        
        float absX = Mathf.Abs(lastMovementDirection.x);
        float absY = Mathf.Abs(lastMovementDirection.y);
        
        if (absY > absX)
        {
            // Mouvement principalement vertical
            if (lastMovementDirection.y > 0)
            {
                currentOffset = itemOffsetUp;
                currentRotation = itemRotationUp;
            }
            else
            {
                currentOffset = itemOffsetDown;
                currentRotation = itemRotationDown;
            }
        }
        else
        {
            // Mouvement horizontal
            if (spriteRenderer.flipX)
            {
                currentOffset = itemOffsetRight;
                currentRotation = itemRotationRight;
            }
            else
            {
                currentOffset = itemOffsetLeft;
                currentRotation = itemRotationLeft;
            }
        }
        
        item.transform.localPosition = currentOffset;
        Vector3 eulerAngles = item.transform.localEulerAngles;
        eulerAngles.z = currentRotation;
        item.transform.localEulerAngles = eulerAngles;
        Debug.Log($"[TopDownPlayerController] Item parenté et positionné - Pas de changement d'animation");
        
        return true;
    }

    /// <summary>
    /// Dépose l'item actuellement équippé.
    /// IMPORTANT: Conserve l'état d'animation actuel du personnage (nage/marche)
    /// </summary>
    public void DropItem()
    {
        if (equippedItem != null)
        {
            // Log l'état actuel du personnage avant de déposer
            CharacterAnimator playerAnimator = GetComponent<CharacterAnimator>();
            if (playerAnimator != null)
            {
                bool wasSwimming = playerAnimator.IsSwimming || playerAnimator.IsSwimmingDeep;
                Debug.Log($"[TopDownPlayerController] État du personnage: {(wasSwimming ? "NAGE" : "MARCHE")} - État CONSERVÉ après drop");
            }
            
            // Détache l'item du joueur
            equippedItem.transform.SetParent(null);
            equippedItem = null;
            hasItem = false;
            
            Debug.Log($"[TopDownPlayerController] Item déposé - Pas de changement d'animation");
        }
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
