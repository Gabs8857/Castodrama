using UnityEngine;
using UnityEngine.U2D.Animation;

/// <summary>
/// Script pour gérer les animations du Castor en fonction du mouvement
/// Switch entre "Walk" et "Swim" selon la catégorie du Sprite Library
/// Support pour multiple frames
/// </summary>
public class CharacterAnimator : MonoBehaviour
{
    [SerializeField]
    private float frameSwitchSpeed = 0.5f; // Temps entre les frames

    [SerializeField]
    private string walkCategoryName = "Walk"; // Catégorie pour la marche

    [SerializeField]
    private string swimCategoryName = "Swim"; // Catégorie pour la nage

    [SerializeField]
    private string[] walkFrameNames = { "Frame1", "Frame2", "Frame3" }; // Frames de marche (3 frames)

    [SerializeField]
    private string[] swimFrameNames = { "Frame1", "Frame2" }; // Frames de nage (2 frames)

    private SpriteResolver spriteResolver;
    private Rigidbody2D rb;
    private float timeSinceLastSwitch = 0f;
    private int currentFrameIndex = 0;
    private bool isSwimming = false;
    private bool isMoving = false;

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

        // Vérifie si le personnage se déplace
        isMoving = rb.linearVelocity.sqrMagnitude > 0.01f;

        // Si on ne bouge pas, pas d'animation
        if (!isMoving)
        {
            timeSinceLastSwitch = 0f;
            return;
        }

        timeSinceLastSwitch += Time.deltaTime;

        if (timeSinceLastSwitch >= frameSwitchSpeed)
        {
            SwitchFrame();
            timeSinceLastSwitch = 0f;
        }
    }

    private void SwitchFrame()
    {
        string[] frameNames = isSwimming ? swimFrameNames : walkFrameNames;
        string categoryName = isSwimming ? swimCategoryName : walkCategoryName;
        
        string labelToSet = frameNames[currentFrameIndex];
        spriteResolver.SetCategoryAndLabel(categoryName, labelToSet);
        
        currentFrameIndex = (currentFrameIndex + 1) % frameNames.Length;
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
}
