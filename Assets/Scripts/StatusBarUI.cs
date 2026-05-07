using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gestionnaire unifié des barres d'état UI (Faim + Danger)
/// Remplace TopDownHungerBarUI.cs et TopDownDangerBarUI.cs
/// 
/// Gère:
/// - Barre de faim circulaire (haut centre)
/// - Barre de danger circulaire (bas gauche)
/// - Mise à jour en temps réel selon les systèmes TopDownHunger et TopDownDanger
/// </summary>
public class StatusBarUI : MonoBehaviour
{
    // ==================== FAIM ====================
    [SerializeField]
    private TopDownHunger hungerSystem;

    [SerializeField]
    private Image hungerBarFill;

    [SerializeField]
    private Image hungerBarBackground;

    [SerializeField]
    private Sprite hungerBackgroundSprite;

    [SerializeField]
    private Image hungerCenterIcon;

    [SerializeField]
    private bool hungerFixedOnScreen = true;

    [SerializeField]
    private RectTransform hungerFixedCanvasPoint;

    [SerializeField]
    private Vector2 hungerFixedAnchoredPosition = new Vector2(0f, 84f);

    [SerializeField]
    private Vector2 hungerBarSize = new Vector2(128f, 128f);

    // ==================== DANGER ====================
    [SerializeField]
    private TopDownDanger dangerSystem;

    [SerializeField]
    private Image dangerBarFill;

    [SerializeField]
    private Image dangerBarBackground;

    [SerializeField]
    private Vector2 dangerAnchoredPosition = new Vector2(132f, 124f);

    [SerializeField]
    private Vector2 dangerBarSize = new Vector2(96f, 96f);

    [SerializeField]
    private float dangerRingThicknessFactor = 0.34f;

    private RectTransform hungerRectTransform;
    private RectTransform dangerRectTransform;

    private void Awake()
    {
        hungerRectTransform = hungerBarFill?.GetComponent<RectTransform>();
        dangerRectTransform = dangerBarFill?.GetComponent<RectTransform>();
    }

    private void Start()
    {
        // Trouve les systèmes s'ils ne sont pas assignés
        if (hungerSystem == null)
        {
            GameObject playerObject = GameObject.Find("Player");
            if (playerObject != null)
            {
                hungerSystem = playerObject.GetComponent<TopDownHunger>();
            }
        }

        if (dangerSystem == null)
        {
            GameObject playerObject = GameObject.Find("Player");
            if (playerObject != null)
            {
                dangerSystem = playerObject.GetComponent<TopDownDanger>();
            }
        }

        // Configure la HungerBar - cercle qui se remplit
        if (hungerBarFill != null)
        {
            hungerBarFill.fillMethod = Image.FillMethod.Radial360;
            hungerBarFill.fillOrigin = (int)Image.Origin360.Top;
            hungerBarFill.color = new Color(0f, 1f, 0f, 1f); // Vert
            
            // Cherche la position de la HungerBar
            RectTransform hungerBarRect = hungerBarFill.GetComponent<RectTransform>();
            if (hungerBarRect != null)
            {
                hungerBarRect.anchoredPosition = hungerFixedAnchoredPosition;
                hungerBarRect.sizeDelta = new Vector2(120f, 120f); // Taille du cercle
            }
        }

        if (dangerRectTransform != null)
        {
            dangerRectTransform.anchoredPosition = dangerAnchoredPosition;
            dangerRectTransform.sizeDelta = dangerBarSize;
            
            // Rendre invisible pour le moment
            if (dangerBarFill != null)
            {
                dangerBarFill.enabled = false;
            }
            if (dangerBarBackground != null)
            {
                dangerBarBackground.enabled = false;
            }
        }
    }

    private void Update()
    {
        // Mets à jour la barre de faim
        if (hungerSystem != null && hungerBarFill != null)
        {
            float hungerFillAmount = hungerSystem.CurrentHunger / hungerSystem.MaxHunger;
            hungerBarFill.fillAmount = hungerFillAmount;
        }

        // Mets à jour la barre de danger
        if (dangerSystem != null && dangerBarFill != null)
        {
            float dangerFillAmount = dangerSystem.CurrentDanger / dangerSystem.MaxDanger;
            dangerBarFill.fillAmount = dangerFillAmount;
        }
    }

    /// <summary>
    /// Retourne la valeur de faim actuelle
    /// </summary>
    public float GetHungerValue()
    {
        if (hungerSystem != null)
        {
            return hungerSystem.CurrentHunger / hungerSystem.MaxHunger;
        }
        return 0f;
    }

    /// <summary>
    /// Retourne la valeur de danger actuelle
    /// </summary>
    public float GetDangerValue()
    {
        if (dangerSystem != null)
        {
            return dangerSystem.CurrentDanger / dangerSystem.MaxDanger;
        }
        return 0f;
    }
}
