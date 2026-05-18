using UnityEngine;
using UnityEngine.U2D.Animation;

/// <summary>
/// Gestionnaire d'animations du Castor
/// Gère:
/// - Les animations de marche (3 frames) selon le mouvement
/// - Les animations de nage (2 frames) dans l'eau
/// - Les animations de nage profonde (2 frames) au fond de rivière
/// - Le switch automatique entre les états via la Sprite Library Unity 2D Animation
/// 
/// Implémente IZoneDetectable pour réagir aux changements de zones (entrée/sortie d'eau)
/// Récupère aussi l'état des zones via TopDownPlayerController.ZoneManager
/// </summary>
public class CharacterAnimator : MonoBehaviour, IZoneDetectable
{
    [SerializeField]
    private float frameSwitchSpeed = 0.1f; // Temps entre les frames (~10 FPS pour animations fluides)

    [SerializeField]
    private float directionSmoothingSpeed = 12f; // Vitesse de lissage (élimine le ping entre directions)

    [SerializeField]
    private string walkCategoryName = "Walk"; // Catégorie pour la marche

    [SerializeField]
    private string swimCategoryName = "Swim"; // Catégorie pour la nage horizontale/diagonale

    [SerializeField]
    private string swimUpCategoryName = "Swim_Up"; // Catégorie pour la nage vers le haut

    [SerializeField]
    private string swimDownCategoryName = "Swim_Down"; // Catégorie pour la nage vers le bas

    [SerializeField]
    private string deepSwimCategoryName = "deep_swim"; // Catégorie pour la nage profonde horizontale/diagonale

    [SerializeField]
    private string deepSwimUpCategoryName = "deep_swim_Up"; // Catégorie pour la nage profonde vers le haut

    [SerializeField]
    private string deepSwimDownCategoryName = "deep_swim_Down"; // Catégorie pour la nage profonde vers le bas

    [SerializeField]
    private string[] walkFrameNames = { "Frame1", "Frame2", "Frame3" }; // Frames de marche (3 frames)

    [SerializeField]
    private string[] swimFrameNames = { "Frame1", "Frame2" }; // Frames de nage (2 frames)

    [SerializeField]
    private string[] swimUpFrameNames = { "Frame1", "Frame2" }; // Frames de nage vers le haut (2 frames)

    [SerializeField]
    private string[] swimDownFrameNames = { "Frame1", "Frame2" }; // Frames de nage vers le bas (2 frames)

    [SerializeField]
    private string[] deepSwimFrameNames = { "Frame1", "Frame2" }; // Frames de nage profonde (2 frames)

    [SerializeField]
    private string[] deepSwimUpFrameNames = { "Frame1", "Frame2" }; // Frames de nage profonde vers le haut (2 frames)

    [SerializeField]
    private string[] deepSwimDownFrameNames = { "Frame1", "Frame2" }; // Frames de nage profonde vers le bas (2 frames)

    [SerializeField]
    private string walkUpCategoryName = "Walk_Up"; // Catégorie pour la montée

    [SerializeField]
    private string walkDownCategoryName = "Walk_Down"; // Catégorie pour la descente

    [SerializeField]
    private string diveCategoryName = "Dive"; // Catégorie pour le plongeon (walk → swim)

    [SerializeField]
    private string[] diveFrameNames = { "Frame1" }; // Frames de plongeon (1 frame)

    [SerializeField]
    private string diveExitCategoryName = "DiveExit"; // Catégorie pour la sortie d'eau (swim → walk)

    [SerializeField]
    private string[] diveExitFrameNames = { "Frame1" }; // Frames de sortie d'eau (1 frame)

    [SerializeField]
    private float transitionAnimationDuration = 0.4f; // Durée totale de la transition (en secondes)

    [SerializeField]
    private string[] walkUpFrameNames = { "Frame1", "Frame2", "Frame3" }; // Frames de montée (3 frames)

    [SerializeField]
    private string[] walkDownFrameNames = { "Frame1", "Frame2", "Frame3" }; // Frames de descente (3 frames)

    private SpriteResolver spriteResolver;
    private Rigidbody2D rb;
    private float timeSinceLastSwitch = 0f;
    private int currentFrameIndex = 0;
    private bool isSwimming = false;
    private bool isSwimmingDeep = false;
    private bool isMoving = false;
    private bool isDiving = false; // Animation de plongeon en cours
    private bool isDivingExit = false; // Animation de sortie d'eau en cours
    private float transitionTimeRemaining = 0f; // Temps restant pour la transition
    private Vector2 lastMovementDirection = Vector2.down; // Dernière direction de mouvement
    private Vector2 smoothedMovementDirection = Vector2.down; // Direction lissée pour éviter les saccades
    private string currentCategoryName = "Walk"; // Catégorie actuellement utilisée

    private void Awake()
    {
        spriteResolver = GetComponent<SpriteResolver>();
        rb = GetComponent<Rigidbody2D>();
        
        if (spriteResolver == null)
        {
            Debug.LogError($"[CharacterAnimator] ERREUR: Pas de SpriteResolver trouvé sur {gameObject.name}!");
        }
        
        if (rb == null)
        {
            Debug.LogError($"[CharacterAnimator] ERREUR: Pas de Rigidbody2D trouvé sur {gameObject.name}!");
        }
    }

    private void Update()
    {
        if (spriteResolver == null || rb == null)
            return;

        // Gère le décompte des animations de transition
        if (isDiving || isDivingExit)
        {
            transitionTimeRemaining -= Time.deltaTime;
            if (transitionTimeRemaining <= 0f)
            {
                // Transition terminée
                if (isDiving)
                {
                    isDiving = false;
                    StartSwimming(); // Passe à la nage
                    Debug.Log("[CharacterAnimator] Plongeon terminé → Mode NAGE");
                }
                else if (isDivingExit)
                {
                    isDivingExit = false;
                    currentFrameIndex = 0;
                    timeSinceLastSwitch = 0f;
                    Debug.Log("[CharacterAnimator] Sortie d'eau terminée → Mode MARCHE");
                }
            }
        }

        // Vérifie si le personnage se déplace
        isMoving = rb.linearVelocity.sqrMagnitude > 0.01f;

        // Mémorise la dernière direction de mouvement (pour les animations)
        if (isMoving)
        {
            lastMovementDirection = rb.linearVelocity.normalized;
        }

        // 🔥 LISSE LA DIRECTION pour éviter les changements brusques (élimine le ping)
        smoothedMovementDirection = Vector2.Lerp(
            smoothedMovementDirection,
            lastMovementDirection,
            directionSmoothingSpeed * Time.deltaTime
        );

        // En nage/transitions : animer peu importe la vitesse
        // En marche : animer seulement si on bouge
        bool shouldAnimate = isSwimming || isSwimmingDeep || isDiving || isDivingExit || isMoving;

        if (!shouldAnimate)
        {
            timeSinceLastSwitch = 0f;
            return;
        }

        // 🔥 DÉTECTE LE CHANGEMENT DE DIRECTION ET CHANGE IMMÉDIATEMENT
        string nextCategory = GetCurrentCategory();
        if (nextCategory != currentCategoryName)
        {
            // Catégorie a changé ! Réinitialise et change immédiatement
            currentCategoryName = nextCategory;
            currentFrameIndex = 0;
            timeSinceLastSwitch = 0f;
            SwitchFrame();
            return;
        }

        timeSinceLastSwitch += Time.deltaTime;

        if (timeSinceLastSwitch >= frameSwitchSpeed)
        {
            SwitchFrame();
            timeSinceLastSwitch = 0f;
        }
    }

    /// <summary>
    /// Détermine la catégorie d'animation à utiliser selon l'état
    /// </summary>
    private string GetCurrentCategory()
    {
        // Transitions de plongeon/sortie d'eau (toujours avant les autres états)
        if (isDiving)
            return diveCategoryName;
        
        if (isDivingExit)
            return diveExitCategoryName;

        // Détecte si le mouvement est plus vertical que horizontal
        float absX = Mathf.Abs(smoothedMovementDirection.x);
        float absY = Mathf.Abs(smoothedMovementDirection.y);
        
        if (isSwimmingDeep)
        {
            // En nage profonde: détermine la direction
            if (absY > absX)
            {
                if (smoothedMovementDirection.y > 0)
                    return deepSwimUpCategoryName;
                else
                    return deepSwimDownCategoryName;
            }
            return deepSwimCategoryName;
        }
        
        if (isSwimming)
        {
            // En nage: détermine la direction
            if (absY > absX)
            {
                if (smoothedMovementDirection.y > 0)
                    return swimUpCategoryName;
                else
                    return swimDownCategoryName;
            }
            return swimCategoryName;
        }
        
        // En marche: détermine la direction
        if (absY > absX)
        {
            // Mouvement principalement vertical
            if (smoothedMovementDirection.y > 0)
                return walkUpCategoryName; // Montée
            else
                return walkDownCategoryName; // Descente
        }
        
        return walkCategoryName; // Mouvement horizontal ou diagonal
    }

    private void SwitchFrame()
    {
        string categoryName = GetCurrentCategory();
        
        string[] frameNames = GetFrameNamesForCategory(categoryName);
        string labelToSet = frameNames[currentFrameIndex];
        
        try
        {
            spriteResolver.SetCategoryAndLabel(categoryName, labelToSet);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[CharacterAnimator] ERROR setting category/label: {e.Message}");
        }
        
        currentFrameIndex = (currentFrameIndex + 1) % frameNames.Length;
    }

    /// <summary>
    /// Retourne le tableau de frames pour une catégorie donnée
    /// </summary>
    private string[] GetFrameNamesForCategory(string categoryName)
    {
        // Transitions
        if (categoryName == diveCategoryName)
            return diveFrameNames;
        if (categoryName == diveExitCategoryName)
            return diveExitFrameNames;
        
        // Nage profonde
        if (categoryName == deepSwimUpCategoryName)
            return deepSwimUpFrameNames;
        if (categoryName == deepSwimDownCategoryName)
            return deepSwimDownFrameNames;
        if (categoryName == deepSwimCategoryName)
            return deepSwimFrameNames;
        
        // Nage normale
        if (categoryName == swimUpCategoryName)
            return swimUpFrameNames;
        if (categoryName == swimDownCategoryName)
            return swimDownFrameNames;
        if (categoryName == swimCategoryName)
            return swimFrameNames;
        
        // Marche
        if (categoryName == walkUpCategoryName)
            return walkUpFrameNames;
        if (categoryName == walkDownCategoryName)
            return walkDownFrameNames;
        
        return walkFrameNames; // Par défaut
    }

    /// <summary>
    /// Active l'animation de nage
    /// </summary>
    public void StartSwimming()
    {
        if (!isSwimming)
        {
            isSwimming = true;
            currentFrameIndex = 0;
            timeSinceLastSwitch = 0f;
            Debug.Log($"[CharacterAnimator] Mode NAGE activé");
        }
    }

    /// <summary>
    /// Désactive l'animation de nage et repasse à la marche
    /// </summary>
    public void StopSwimming()
    {
        if (isSwimming)
        {
            isSwimming = false;
            currentFrameIndex = 0;
            timeSinceLastSwitch = 0f;
            Debug.Log($"[CharacterAnimator] Mode MARCHE activé");
        }
    }

    /// <summary>
    /// Retourne si le personnage est en train de nager
    /// </summary>
    public bool IsSwimming => isSwimming;

    /// <summary>
    /// Retourne si le personnage est en train de nager profondément
    /// </summary>
    public bool IsSwimmingDeep => isSwimmingDeep;

    /// <summary>
    /// Active l'animation de nage profonde
    /// </summary>
    public void StartSwimmingDeep()
    {
        if (!isSwimmingDeep)
        {
            isSwimmingDeep = true;
            isSwimming = false; // Désactive la nage normale
            currentFrameIndex = 0;
            timeSinceLastSwitch = 0f;
            Debug.Log($"[CharacterAnimator] Mode NAGE PROFONDE activé");
        }
    }

    /// <summary>
    /// Désactive l'animation de nage profonde et repasse à la marche
    /// </summary>
    public void StopSwimmingDeep()
    {
        if (isSwimmingDeep)
        {
            isSwimmingDeep = false;
            currentFrameIndex = 0;
            timeSinceLastSwitch = 0f;
            Debug.Log($"[CharacterAnimator] Mode MARCHE activé (sortie nage profonde)");
        }
    }

    /// <summary>
    /// Démarre l'animation de plongeon (walk → swim)
    /// </summary>
    public void StartDive()
    {
        if (!isDiving)
        {
            isDiving = true;
            isDivingExit = false;
            transitionTimeRemaining = transitionAnimationDuration;
            currentFrameIndex = 0;
            timeSinceLastSwitch = 0f;
            currentCategoryName = diveCategoryName;
            Debug.Log($"[CharacterAnimator] Animation PLONGEON démarrée");
        }
    }

    /// <summary>
    /// Démarre l'animation de sortie d'eau (swim → walk)
    /// </summary>
    public void StartDiveExit()
    {
        if (!isDivingExit)
        {
            isDivingExit = true;
            isDiving = false;
            transitionTimeRemaining = transitionAnimationDuration;
            currentFrameIndex = 0;
            timeSinceLastSwitch = 0f;
            currentCategoryName = diveExitCategoryName;
            Debug.Log($"[CharacterAnimator] Animation SORTIE D'EAU démarrée");
        }
    }

    /// <summary>
    /// Implémentation de IZoneDetectable - Appelé quand le perso entre dans une zone
    /// </summary>
    public void OnEnterZone(ZoneType zoneType)
    {
        Debug.Log($"[CharacterAnimator] Entrée dans la zone: {zoneType}");
        
        if (zoneType == ZoneType.Water)
        {
            StartSwimming();
        }
        else if (zoneType == ZoneType.EntRiviere)
        {
            // Décide si on plonge ou on sort selon l'état actuel
            if (isSwimming)
            {
                StartDiveExit(); // On est en train de nager, on sort
            }
            else
            {
                StartDive(); // On est en train de marcher, on plonge
            }
        }
    }

    /// <summary>
    /// Implémentation de IZoneDetectable - Appelé quand le perso sort d'une zone
    /// </summary>
    public void OnExitZone(ZoneType zoneType)
    {
        Debug.Log($"[CharacterAnimator] Sortie de la zone: {zoneType}");
        
        if (zoneType == ZoneType.Water)
        {
            StopSwimming();
        }
    }

    /// <summary>
    /// Retourne si l'objet est actuellement dans une zone
    /// </summary>
    public bool IsInZone(ZoneType zoneType)
    {
        TopDownPlayerController playerController = GetComponent<TopDownPlayerController>();
        if (playerController != null)
        {
            return playerController.ZoneManager.IsInZone(zoneType);
        }
        return false;
    }
}
