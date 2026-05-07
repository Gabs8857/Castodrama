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
    private Vector3 itemOffset = new Vector3(0.3f, 0.2f, 0); // Position relative par rapport au joueur

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
        
        // Initialise le gestionnaire de zones
        zoneManager = new ZoneDetectionManager(this);
        
        Debug.Log($"[TopDownPlayerController] Initialisation complète");
    }

    protected override void Update()
    {
        base.Update(); // Appelle Update du PlayerController (lecture des entrées)
        
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
        item.transform.localPosition = itemOffset;
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
