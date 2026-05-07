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

        // Configure les positions fixes si activé
        if (hungerFixedOnScreen && hungerRectTransform != null && hungerFixedCanvasPoint != null)
        {
            hungerRectTransform.anchoredPosition = hungerFixedAnchoredPosition;
        }

        if (dangerRectTransform != null)
        {
            dangerRectTransform.anchoredPosition = dangerAnchoredPosition;
            dangerRectTransform.sizeDelta = dangerBarSize;
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
