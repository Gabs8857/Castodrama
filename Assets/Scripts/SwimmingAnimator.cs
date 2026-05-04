using UnityEngine;
using UnityEngine.U2D.Animation;

/// <summary>
/// Script pour animer le Castor/personnage en train de nager
/// Alterne entre 2 frames avec une vitesse configurable
/// </summary>
public class SwimmingAnimator : MonoBehaviour
{
    [SerializeField]
    private float frameSwitchSpeed = 0.5f; // Temps en secondes entre les frames

    [SerializeField]
    private string categoryName = "Swim"; // Catégorie Sprite Library (ex: "Swim")

    [SerializeField]
    private string frame1Name = "Frame1";

    [SerializeField]
    private string frame2Name = "Frame2";

    private SpriteResolver spriteResolver;
    private float timeSinceLastSwitch = 0f;
    private bool isFrame1 = true;

    private void Awake()
    {
        spriteResolver = GetComponent<SpriteResolver>();
        if (spriteResolver == null)
        {
            Debug.LogError($"[SwimmingAnimator] ERREUR: Pas de SpriteResolver trouvé sur {gameObject.name}!");
        }
        
        // Désactiver le script par défaut (sera activé quand on rentre dans l'eau)
        this.enabled = false;
    }

    private void Update()
    {
        if (spriteResolver == null)
            return;

        timeSinceLastSwitch += Time.deltaTime;

        if (timeSinceLastSwitch >= frameSwitchSpeed)
        {
            SwitchFrame();
            timeSinceLastSwitch = 0f;
        }
    }

    private void SwitchFrame()
    {
        string labelToSet = isFrame1 ? frame2Name : frame1Name;
        spriteResolver.SetCategoryAndLabel(categoryName, labelToSet);
        isFrame1 = !isFrame1;
        
        Debug.Log($"[SwimmingAnimator] Frame switched to: {labelToSet}");
    }
}
